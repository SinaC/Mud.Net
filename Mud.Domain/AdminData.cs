namespace Mud.Domain
{
    public class AdminData : PlayerData
    {
        public AdminLevels AdminLevel { get; set; }
        public WiznetFlags WiznetFlags { get; set; }

        // TODO: extra fields
    }
}
