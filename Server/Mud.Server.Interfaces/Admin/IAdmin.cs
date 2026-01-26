using Mud.Domain;
using Mud.Domain.SerializationData.Account;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Interfaces.Admin;

public interface IAdmin : IPlayer
{
    void Initialize(Guid id, string name, string password, AdminLevels level, IReadOnlyDictionary<string, string> aliases, IEnumerable<AvatarMetaData> avatarMetaDatas);

    AdminLevels Level { get; }

    IWiznetFlags WiznetFlags { get; }
    void AddWiznet(IWiznetFlags wiznetFlags);
    void RemoveWiznet(IWiznetFlags wiznetFlags);

    IEntity? Incarnating { get; }
    bool StartIncarnating(IEntity entity);
    void StopIncarnating();
}
