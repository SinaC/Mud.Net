using Mud.Domain;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Interfaces.Admin
{
    public interface IAdmin : IPlayer
    {
        AdminLevels Level { get; }

        WiznetFlags WiznetFlags { get; }

        IEntity Incarnating { get; }

        void StopIncarnating();
    }
}
