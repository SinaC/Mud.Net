using System;
using System.Collections.Generic;
using Mud.DataStructures.Trie;
using Mud.Server.Blueprints;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Server;

namespace Mud.Server.Room
{
    public class Room : EntityBase, IRoom
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> RoomCommands;

        private readonly List<ICharacter> _people;
        private readonly List<IItem> _content;
        private readonly IExit[] _exits; // see ServerOptions.ExitDirections

        static Room()
        {
            RoomCommands = CommandHelpers.GetCommands(typeof (Room));
        }

        public Room(Guid guid, string name)
            : base(guid, name)
        {
            _people = new List<ICharacter>();
            _content = new List<IItem>();
            _exits = new IExit[ServerOptions.ExitCount];
        }

        public Room(Guid guid, string name, string description)
            :base(guid, name, description)
        {
            _people = new List<ICharacter>();
            _content = new List<IItem>();
            _exits = new IExit[ServerOptions.ExitCount];
        }

        public Room(Guid guid, RoomBlueprint blueprint)
            : base(guid, blueprint.Name, blueprint.Description)
        {
            _people = new List<ICharacter>();
            _content = new List<IItem>();
            _exits = new IExit[ServerOptions.ExitCount];
        }

        #region IRoom

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return RoomCommands; }
        }

        #endregion

        public override string DisplayName
        {
            get { return StringHelpers.UpperFirstLetter(Name); }
        }

        #endregion

        #region IContainer

        public IReadOnlyCollection<IItem> Content
        {
            get { return _content.AsReadOnly(); }
        }

        public bool Put(IItem obj)
        {
            // TODO: check if already in a container
            _content.Add(obj);
            return true;
        }

        public bool Get(IItem obj)
        {
            bool removed = _content.Remove(obj);
            return removed;
        }

        #endregion

        public RoomBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor

        public IReadOnlyCollection<ICharacter> People
        {
            get { return _people.AsReadOnly(); }
        }
        
        public IExit[] Exits { get { return _exits; } }

        public IExit Exit(ServerOptions.ExitDirections direction)
        {
            return _exits[(int) direction];
        }

        public IRoom GetRoom(ServerOptions.ExitDirections direction)
        {
            IExit exit = Exit(direction);
            return exit != null ? exit.Destination : null;
        }

        public void Enter(ICharacter character)
        {
            // TODO: check if not already in room
            _people.Add(character);
        }

        public void Leave(ICharacter character)
        {
            // TODO: check if in room
            bool removed = _people.Remove(character);
        }

        #endregion
    }
}
