namespace Mud.Repository.Mongo.Domain
{
    public class RoomHealRateAffectData : AffectDataBase
    {
        public int Operator { get; set; }

        public int Modifier { get; set; }
    }
}
