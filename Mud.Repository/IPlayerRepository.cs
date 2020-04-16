using Mud.Domain;
using System.Collections.Generic;

namespace Mud.Repository
{
    public interface IPlayerRepository
    {
        PlayerData Load(string playerName);
        void Save(PlayerData playerData);
        void Delete(string playerName);
        IEnumerable<string> GetAvatarNames();
    }
}
