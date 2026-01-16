using Mud.Server.Domain;

namespace Mud.Server.Interfaces.Affect.Room;

public interface IRoomHealRateAffect : IRoomAffect
{
    int Modifier { get; set; }
    AffectOperators Operator { get; set; }
}
