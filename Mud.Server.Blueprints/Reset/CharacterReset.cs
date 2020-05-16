namespace Mud.Server.Blueprints.Reset
{
    public class CharacterReset : ResetBase
    {
        public int CharacterId { get; set; }
        public int GlobalLimit { get; set; }
        public int LocalLimit { get; set; }
    }
}
