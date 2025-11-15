using Mud.Domain;

namespace Mud.Server.Interfaces.Affect;

public interface IRoomHealRateAffect : IRoomAffect
{
    int Modifier { get; set; }
    AffectOperators Operator { get; set; }
}
