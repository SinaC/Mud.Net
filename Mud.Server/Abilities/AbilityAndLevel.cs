namespace Mud.Server.Abilities
{
    public class AbilityAndLevel
    {
        public int Level { get; }
        public IAbility Ability { get; }

        public AbilityAndLevel(int level, IAbility ability)
        {
            Level = level;
            Ability = ability;
        }
    }
}
