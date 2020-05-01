namespace Mud.Repository.Mongo.Domain
{
    public class AuraData
    {
        public int AbilitiId { get; set; }

        // TODO: source

        public int Level { get; set; }

        public int PulseLeft { get; set; }

        public int AuraFlags { get; set; }

        public AffectDataBase[] Affects { get; set; }
    }
}
