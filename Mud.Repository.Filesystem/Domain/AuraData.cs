namespace Mud.Repository.Filesystem.Domain
{
    public class AuraData
    {
        public int AbilityId { get; set; }

        // TODO: source

        public int Level { get; set; }

        public int PulseLeft { get; set; }

        public int AuraFlags { get; set; }

        public AffectDataBase[] Affects { get; set; }
    }
}
