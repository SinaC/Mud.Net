namespace Mud.Domain
{
    public class RoomResourceRateAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; }

        public int Modifier { get; set; }
    }
}
