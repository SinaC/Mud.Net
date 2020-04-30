using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Aura;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.Entity;
using Mud.Server.Input;

namespace Mud.Server.Room
{
    public class Room : EntityBase, IRoom
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> RoomCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(GetCommands<Room>);

        private readonly List<ICharacter> _people;
        private readonly List<IItem> _content;

        public Room(Guid guid, RoomBlueprint blueprint, IArea area)
            : base(guid, blueprint.Name, blueprint.Description)
        {
            Blueprint = blueprint;
            _people = new List<ICharacter>();
            _content = new List<IItem>();
            Exits = new IExit[ExitDirectionsExtensions.ExitCount];

            Area = area;
            Area.AddRoom(this);
        }

        #region IRoom

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => RoomCommands.Value;

        #endregion

        public override string DisplayName => Name.UpperFirstLetter();

        public override string DebugName => $"{DisplayName}[{Blueprint.Id}]";

        // Recompute
        public override void Recompute()
        {
            // 0) Reset
            ResetAttributes();

            // 1) Apply own auras
            ApplyAuras(this);

            // 2) Apply people auras
            foreach (ICharacter character in People)
                ApplyAuras(character);

            // 3) Apply content auras
            foreach (IItem item in Content)
                ApplyAuras(item);
        }

        //
        public override void OnRemoved()
        {
            base.OnRemoved();
            Blueprint = null;
            _people.Clear();
            for (int i = 0; i < Exits.Length; i++)
                Exits[i] = null;
            _content.Clear();
        }

        #endregion

        #region IContainer

        public IEnumerable<IItem> Content => _content.Where(x => x.IsValid);

        public bool PutInContainer(IItem obj)
        {
            // TODO: check if already in a container
            _content.Insert(0, obj);
            return true;
        }

        public bool GetFromContainer(IItem obj)
        {
            bool removed = _content.Remove(obj);
            return removed;
        }

        #endregion

        public RoomBlueprint Blueprint { get; private set; }

        public IReadOnlyDictionary<string, string> ExtraDescriptions => Blueprint.ExtraDescriptions;

        public RoomFlags BaseRoomFlags { get; protected set; }
        public RoomFlags CurrentRoomFlags { get; protected set; }

        public IArea Area { get; }

        public IEnumerable<ICharacter> People => _people.Where(x => x.IsValid);

        public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => People.OfType<INonPlayableCharacter>();

        public IEnumerable<IPlayableCharacter> PlayableCharacters => People.OfType<IPlayableCharacter>();

        public IEnumerable<(INonPlayableCharacter character, TBlueprint blueprint)> GetNonPlayableCharacters<TBlueprint>()
            where TBlueprint : CharacterBlueprintBase
        {
            foreach (var character in NonPlayableCharacters.Where(x => x.Blueprint is TBlueprint))
                yield return (character, character.Blueprint as TBlueprint);
        }

        public bool IsPrivate
        {
            get
            {
                // TODO: ownership
                int count = People.Count();
                if (CurrentRoomFlags.HasFlag(RoomFlags.Private) && count >= 2)
                    return true;
                if (CurrentRoomFlags.HasFlag(RoomFlags.Solitary) && count >= 1)
                    return true;
                if (CurrentRoomFlags.HasFlag(RoomFlags.ImpOnly))
                    return true;
                return false;
            }
        }

        public IExit[] Exits { get; }

        public IExit Exit(ExitDirections direction)
        {
            return Exits[(int) direction];
        }

        public IRoom GetRoom(ExitDirections direction)
        {
            IExit exit = Exit(direction);
            return exit?.Destination;
        }

        public bool Enter(ICharacter character)
        {
            //if (character.Room != null)
            //{
            //    Log.Default.WriteLine(LogLevels.Error, $"IRoom.Enter: Character {character.DebugName} is already in Room {character.Room.DebugName}");
            //    return false;
            //}
            if (_people.Contains(character))
                Log.Default.WriteLine(LogLevels.Error, $"IRoom.Enter: Character {character.DebugName} is already in Room {character.Room.DebugName}");
            else
                _people.Add(character);
            // Update location quest
            if (character is IPlayableCharacter playableCharacter)
            {
                foreach(IQuest quest in playableCharacter.Quests)
                    quest.Update(this);
            }
            return true;
        }

        public bool Leave(ICharacter character)
        {
            // TODO: check if in room
            bool removed = _people.Remove(character);
            return removed;
        }

        public void ApplyAffect(RoomFlagsAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                case AffectOperators.Or:
                    CurrentRoomFlags |= affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    CurrentRoomFlags = affect.Modifier;
                    break;
                case AffectOperators.Nor:
                    CurrentRoomFlags &= ~affect.Modifier;
                    break;
                default:
                    break;
            }
            return;
        }

        #endregion

        protected virtual void ResetAttributes()
        {
            CurrentRoomFlags = BaseRoomFlags;
        }

        protected void ApplyAuras(IEntity entity)
        {
            if (!entity.IsValid)
                return;
            foreach (IAura aura in entity.Auras.Where(x => x.IsValid))
            {
                foreach (IRoomAffect affect in aura.Affects.OfType<IRoomAffect>())
                {
                    affect.Apply(this);
                }
            }
        }

        [Command("test", "!!Test!!")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Room: DoTest");
            return true;
        }

        [Command("look", "Information")]
        protected virtual CommandExecutionResults DoLook(string rawParameters, params CommandParameter[] parameters)
        {
            //TODO: better 'UI'
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("People:");
            foreach (ICharacter character in _people)
                sb.AppendFormatLine($"{character.DisplayName}");
            sb.AppendLine("Items:");
            foreach (IItem item in _content)
                sb.AppendFormatLine($"{item.DisplayName}");
            //
            Send(sb);
            return CommandExecutionResults.Ok;
        }
    }
}
