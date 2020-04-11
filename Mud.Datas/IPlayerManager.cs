using Mud.Datas.DataContracts;

namespace Mud.Datas
{
    public interface IPlayerManager
    {
        PlayerData Load(string playerName);
        void Save(PlayerData data);
        void Delete(string playerName);
    }
}
