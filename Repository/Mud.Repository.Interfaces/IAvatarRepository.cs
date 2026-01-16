using Mud.Domain.SerializationData.Avatar;

namespace Mud.Repository.Interfaces;

public interface IAvatarRepository
{
    AvatarData? Load(string avatarName);
    void Save(AvatarData avatarName);
    void Delete(string avatarName);
    IEnumerable<string> AvatarNames { get; }
}
