using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Mud.DataStructures.Trie;
using Mud.Server.Blueprints;
using Mud.Server.Input;

namespace Mud.Server.Room
{
    public class Room : EntityBase, IRoom
    {
        private static readonly Trie<MethodInfo> RoomCommands;

        private readonly List<ICharacter> _charactersInRoom;
        private readonly IExit[] _exits; // see ServerOptions.ExitDirections

        static Room()
        {
            RoomCommands = CommandHelpers.GetCommands(typeof (Room));
        }

        public Room(Guid guid, string name)
            : base(guid, name)
        {
            _charactersInRoom = new List<ICharacter>();
            _exits = new IExit[ServerOptions.ExitCount];
        }

        #region IRoom

        #region IActor

        public override IReadOnlyTrie<MethodInfo> Commands
        {
            get { return RoomCommands; }
        }

        #endregion

        public RoomBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor

        public IReadOnlyCollection<ICharacter> CharactersInRoom
        {
            get { return new ReadOnlyCollection<ICharacter>(_charactersInRoom); }
        }

        public IExit[] Exits { get { return _exits; } }

        public IExit Exit(ServerOptions.ExitDirections direction)
        {
            return _exits[(int) direction];
        }

        public void Enter(ICharacter character)
        {
            _charactersInRoom.Add(character);
        }

        public void Leave(ICharacter character)
        {
            bool removed = _charactersInRoom.Remove(character);
        }

        #endregion
    }
}
