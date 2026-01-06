using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Interfaces;

public interface IServerPlayerCommand
{
    AvatarData? LoadAvatar(string avatarName);
    void SaveAvatar(AvatarData avatar);
    void DeleteAvatar(string avatarName);

    void Save(IPlayer player);
    void Quit(IPlayer player);
    void Delete(IPlayer player);

}
