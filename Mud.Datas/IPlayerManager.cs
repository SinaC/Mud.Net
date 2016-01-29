namespace Mud.Datas
{
    public interface IPlayerManager
    {
        PlayerData Load(string playerName);
        void Save(PlayerData data);
    }
}
