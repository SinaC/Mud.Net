namespace Mud.POC.Abilities
{
    public class KnownAbility
    {
        public IAbility Ability { get; set; }
        
        public int Level { get; set; } // level at which ability can be learned
        
        public int Learned { get; set; } // practice percentage, 0 means not learned
    }
}
