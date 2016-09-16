using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.Constants;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Room
{
    public class Room : EntityBase, IRoom
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> RoomCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => CommandHelpers.GetCommands(typeof(Room)));

        private readonly List<ICharacter> _people;
        private readonly List<IItem> _content;

        public Room(Guid guid, RoomBlueprint blueprint, IArea area)
            : base(guid, blueprint.Name, blueprint.Description)
        {
            Blueprint = blueprint;
            _people = new List<ICharacter>();
            _content = new List<IItem>();
            Exits = new IExit[ExitHelpers.ExitCount];

            Area = area;
            Area.AddRoom(this);
        }

        #region IRoom

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => RoomCommands.Value;

        #endregion

        public override string DisplayName => StringHelpers.UpperFirstLetter(Name);

        public override string DebugName => $"{DisplayName}[{Blueprint.Id}]";

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

        public IArea Area { get; }

        public IEnumerable<ICharacter> People => _people.Where(x => x.IsValid);

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
            // TODO: check if not already in room
            _people.Add(character);
            // Update location quest
            if (character.Impersonable)
            {
                foreach(IQuest quest in character.Quests)
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

        #endregion

        [Command("test", Category = "!!Test!!")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Room: DoTest");
            return true;
        }

        [Command("look", Category = "Information")]
        protected virtual bool DoLook(string rawParameters, params CommandParameter[] parameters)
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
            return true;
        }
    }
}
