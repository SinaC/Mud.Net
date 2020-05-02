namespace Mud.POC.Abilities
{
    public class Ability : IAbility
    {
        public Ability(AbilityMethodInfo abilityMethodInfo)
        {
            AbilityMethodInfo = abilityMethodInfo;
            Id = abilityMethodInfo.Attribute.Id;
            Name = abilityMethodInfo.Attribute.Name;
            Target = abilityMethodInfo.Attribute.Target;
            PulseWaitTime = abilityMethodInfo.Attribute.PulseWaitTime;
            AbilityFlags = abilityMethodInfo.Attribute.Flags;
            CharacterDispelMessage = abilityMethodInfo.Attribute.CharacterDispelMessage;
            ItemDispelMessage = abilityMethodInfo.Attribute.ItemDispelMessage;
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public AbilityTargets Target { get; private set; }

        public int PulseWaitTime { get; private set; }

        public AbilityFlags AbilityFlags { get; private set; }

        public string CharacterDispelMessage { get; private set; }

        public string ItemDispelMessage { get; private set; }

        public AbilityMethodInfo AbilityMethodInfo { get; }
    }
}
