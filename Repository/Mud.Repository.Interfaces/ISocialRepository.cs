using Mud.Domain.SerializationData;

namespace Mud.Repository.Interfaces;

public interface ISocialRepository
{
    IReadOnlyCollection<SocialData> Load();
}
