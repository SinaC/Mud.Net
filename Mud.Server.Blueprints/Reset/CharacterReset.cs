namespace Mud.Server.Blueprints.Reset
{
    public class CharacterReset : ResetBase // 'M'
    {
        public int CharacterId { get; set; } // arg1
        public int GlobalLimit { get; set; } // arg2
        public int LocalLimit { get; set; } // arg4
    }
}
