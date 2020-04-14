using Mud.Domain;

namespace Mud.Server
{
    public interface IAdmin : IPlayer
    {
        AdminLevels Level { get; }

        WiznetFlags WiznetFlags { get; }

        IEntity Incarnating { get; }

        void StopIncarnating();
    }
}
