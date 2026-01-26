using Mud.Server.Domain;

namespace Mud.Server.Interfaces.Affect.Room;

public interface IRoomResourceRateAffect : IRoomAffect
{
    int Modifier { get; }
    AffectOperators Operator { get; }
}
