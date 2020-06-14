namespace Mud.Repository.Mongo.Domain
{
    public class AdminData : PlayerData
    {
        public int Level { get; set; }

        public int WiznetFlags { get; set; }

        // TODO: extra fields
    }
}
