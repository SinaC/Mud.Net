namespace Mud.Repository.Mongo.Domain
{
    public class RoomResourceRateAffectData : AffectDataBase
    {
        public int Operator { get; set; }

        public int Modifier { get; set; }
    }
}
