using System;
using System.Collections.Generic;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Room
{
    public class Room : EntityBase, IRoom
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> RoomCommands;

        private readonly List<ICharacter> _people;
        private readonly List<IItem> _content;

        static Room()
        {
            RoomCommands = CommandHelpers.GetCommands(typeof (Room));
        }

        public Room(Guid guid, RoomBlueprint blueprint)
            : base(guid, blueprint.Name, blueprint.Description)
        {
            _people = new List<ICharacter>();
            _content = new List<IItem>();
            Exits = new IExit[ExitHelpers.ExitCount];
        }

        #region IRoom

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => RoomCommands;

        #endregion

        public override string DisplayName => StringHelpers.UpperFirstLetter(Name);

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

        public IReadOnlyCollection<IItem> Content => _content.AsReadOnly();

        public bool PutInContainer(IItem obj)
        {
            // TODO: check if already in a container
            _content.Add(obj);
            return true;
        }

        public bool GetFromContainer(IItem obj)
        {
            bool removed = _content.Remove(obj);
            return removed;
        }

        #endregion

        public RoomBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor

        public IReadOnlyCollection<ICharacter> People => _people.AsReadOnly();

        public IExit[] Exits { get; }

        public IExit Exit(ExitDirections direction)
        {
            return Exits[(int) direction];
        }

        public IRoom GetRoom(ExitDirections direction)
        {
            IExit exit = Exit(direction);
            return exit != null ? exit.Destination : null;
        }

        public bool Enter(ICharacter character)
        {
            // TODO: check if not already in room
            _people.Add(character);
            return true;
        }

        public bool Leave(ICharacter character)
        {
            // TODO: check if in room
            bool removed = _people.Remove(character);
            return removed;
        }

        #endregion

        [Command("test")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Room: DoTest" + Environment.NewLine);
            return true;
        }

        [Command("look")]
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
