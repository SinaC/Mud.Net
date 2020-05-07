using Mud.Domain;
using System.Reflection;

namespace Mud.Server.Abilities
{
    public class Ability : IAbility
    {
        public Ability(AbilityKinds kind, int id, string name, AbilityTargets target, int pulseWaitTime, AbilityFlags flags, string characterDispelMessage, string itemDispelMessage)
        {
            Kind = kind;
            Id = id;
            Name = name;
            Target = target;
            PulseWaitTime = pulseWaitTime;
            AbilityFlags = flags;
            CharacterDispelMessage = characterDispelMessage;
            ItemDispelMessage = itemDispelMessage;
        }

        public Ability(AbilityKinds kind, AbilityAttribute attribute, MethodInfo methodInfo)
            : this(kind, attribute.Id, attribute.Name, attribute.Target, attribute.PulseWaitTime, attribute.Flags, attribute.CharacterDispelMessage, attribute.ItemDispelMessage)
        {
            MethodInfo = methodInfo;
        }

        public AbilityKinds Kind { get; }

        public int Id { get; }

        public string Name { get; }

        public AbilityTargets Target { get; }

        public int PulseWaitTime { get; }

        public AbilityFlags AbilityFlags { get; }

        public string CharacterDispelMessage { get; }

        public string ItemDispelMessage { get; }

        public MethodInfo MethodInfo { get; } // null for passive abilities
    }
}
