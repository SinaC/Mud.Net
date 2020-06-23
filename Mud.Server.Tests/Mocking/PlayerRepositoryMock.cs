using Mud.Domain;
using Mud.Repository;
using System;
using System.Collections.Generic;

namespace Mud.Server.Tests.Mocking
{
    public class PlayerRepositoryMock : IPlayerRepository
    {
        public void Delete(string playerName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAvatarNames()
        {
            throw new NotImplementedException();
        }

        public PlayerData Load(string playerName)
        {
            throw new NotImplementedException();
        }

        public void Save(PlayerData playerData)
        {
            throw new NotImplementedException();
        }
    }
}
