namespace Mud.POC.Abilities
{
    public class Ability : IAbility
    {
        public Ability(int id, string name, AbilityTargets target, int pulseWaitTime, AbilityFlags flags, string characterDispelMessage, string itemDispelMessage)
        {
            Id = id;
            Name = name;
            Target = target;
            PulseWaitTime = pulseWaitTime;
            AbilityFlags = flags;
            CharacterDispelMessage = characterDispelMessage;
            ItemDispelMessage = itemDispelMessage;
        }

        public Ability(AbilityMethodInfo abilityMethodInfo)
            : this(abilityMethodInfo.Attribute.Id, abilityMethodInfo.Attribute.Name, abilityMethodInfo.Attribute.Target, abilityMethodInfo.Attribute.PulseWaitTime, abilityMethodInfo.Attribute.Flags, abilityMethodInfo.Attribute.CharacterDispelMessage, abilityMethodInfo.Attribute.ItemDispelMessage)
        {
            AbilityMethodInfo = abilityMethodInfo;
        }

        public int Id { get; }

        public string Name { get; }

        public AbilityTargets Target { get; }

        public int PulseWaitTime { get; }

        public AbilityFlags AbilityFlags { get; }

        public string CharacterDispelMessage { get; }

        public string ItemDispelMessage { get; }

        public AbilityMethodInfo AbilityMethodInfo { get; }
    }
}
