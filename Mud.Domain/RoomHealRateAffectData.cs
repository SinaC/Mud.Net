namespace Mud.Domain;

public class RoomHealRateAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; }

    public required int Modifier { get; set; }
}
