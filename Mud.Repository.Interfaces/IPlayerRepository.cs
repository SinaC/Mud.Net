using Mud.Domain.SerializationData;

namespace Mud.Repository.Interfaces;

public interface IPlayerRepository
{
    PlayerData? Load(string playerName);
    void Save(PlayerData playerData);
    void Delete(string playerName);
    IEnumerable<string> GetAvatarNames();
}
