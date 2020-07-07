namespace Mud.Domain
{
    public class RoomHealRateAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; }

        public int Modifier { get; set; }
    }
}
