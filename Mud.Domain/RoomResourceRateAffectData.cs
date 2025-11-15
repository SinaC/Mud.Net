namespace Mud.Domain;

public class RoomResourceRateAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; }

    public required int Modifier { get; set; }
}
