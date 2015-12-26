using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Mud.DataStructures;

namespace Mud.Server.Room
{
    public class Room : EntityBase, IRoom
    {
        private static readonly IReadOnlyTrie<MethodInfo> RoomCommands;

        private readonly List<ICharacter> _charactersInRoom;

        static Room()
        {
            RoomCommands = CommandHelpers.GetCommands(typeof (Room));
        }

        public Room(Guid guid, string name)
            : base(guid, name)
        {
            _charactersInRoom = new List<ICharacter>();
        }

        #region IRoom

        #region IActor

        public override IReadOnlyTrie<MethodInfo> Commands
        {
            get { return RoomCommands; }
        }

        #endregion

        public IReadOnlyCollection<ICharacter> CharactersInRoom
        {
            get { return new ReadOnlyCollection<ICharacter>(_charactersInRoom); }
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
