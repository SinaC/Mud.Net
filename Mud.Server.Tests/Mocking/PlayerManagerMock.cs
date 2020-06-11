using Mud.Network;
using Mud.Server.Input;
using Mud.Server.Interfaces.Player;
using System;
using System.Collections.Generic;

namespace Mud.Server.Tests.Mocking
{
    internal class PlayerManagerMock : IPlayerManager
    {
        private List<IPlayer> _players = new List<IPlayer>();

        public IEnumerable<IPlayer> Players => _players;

        public IPlayer AddPlayer(IClient client, string name)
        {
            IPlayer player = new Player.Player(Guid.NewGuid(), name);
            _players.Add(player);
            return player;
        }

        public IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch)
        {
            throw new NotImplementedException();
        }
    }
}
