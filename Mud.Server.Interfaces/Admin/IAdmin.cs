using Mud.Domain;

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
