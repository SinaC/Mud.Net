namespace Mud.Server.Abilities
{
    public class AbilityAndLevel
    {
        public int Level { get; private set; }
        public IAbility Ability { get; private set; }

        public AbilityAndLevel(int level, IAbility ability)
        {
            Level = level;
            Ability = ability;
        }
    }
}
