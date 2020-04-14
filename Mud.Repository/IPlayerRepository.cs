using Mud.Domain;

namespace Mud.Repository
{
    public interface IPlayerRepository
    {
        PlayerData Load(string playerName);
        void Save(PlayerData data);
        void Delete(string playerName);
    }
}
