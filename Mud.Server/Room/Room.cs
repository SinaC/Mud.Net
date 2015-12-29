using System;
using System.Collections.Generic;
using Mud.DataStructures.Trie;
using Mud.Server.Blueprints;
using Mud.Server.Entity;
using Mud.Server.Input;

namespace Mud.Server.Room
{
    public class Room : EntityBase, IRoom
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> RoomCommands;

        private readonly List<ICharacter> _people;
        private readonly List<IItem> _inside;
        private readonly IExit[] _exits; // see ServerOptions.ExitDirections

        static Room()
        {
            RoomCommands = CommandHelpers.GetCommands(typeof (Room));
        }

        public Room(Guid guid, string name)
            : base(guid, name)
        {
            _people = new List<ICharacter>();
            _inside = new List<IItem>();
            _exits = new IExit[ServerOptions.ExitCount];
        }

        #region IRoom

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return RoomCommands; }
        }

        #endregion

        #region IContainer

        public IReadOnlyCollection<IItem> Inside
        {
            get { return _inside.AsReadOnly(); }
        }

        public bool Put(IItem obj)
        {
            // TODO: check if already in a container
            _inside.Add(obj);
            return true;
        }

        public bool Get(IItem obj)
        {
            bool removed = _inside.Remove(obj);
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
