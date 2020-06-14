namespace Mud.Repository.Filesystem.Domain
{
    public class AdminData : PlayerData
    {
        public int AdminLevel { get; set; }

        public int WiznetFlags { get; set; }

        // TODO: extra fields
    }
}
