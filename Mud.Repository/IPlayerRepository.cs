using Mud.Domain;

namespace Mud.Repository
{
    public interface IPlayerRepository
    {
        PlayerData Load(string playerName);
        void Save(PlayerData playerData);
        void Delete(string playerName);
    }
}
