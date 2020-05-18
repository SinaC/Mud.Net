using Mud.Domain;
using System.Reflection;

namespace Mud.Server.Abilities
{
    public class Ability : IAbility
    {
        public Ability(AbilityKinds kind, int id, string name, AbilityTargets target, int pulseWaitTime, AbilityFlags flags, string characterWearOffMessage, string itemWearOffMessage, string dispelRoomMessage, string damageNoun, int learnDifficultyMultiplier, int cooldown)
        {
            Kind = kind;
            Id = id;
            Name = name;
            Target = target;
            PulseWaitTime = pulseWaitTime;
            AbilityFlags = flags;
            CharacterWearOffMessage = characterWearOffMessage;
            ItemWearOffMessage = itemWearOffMessage;
            DispelRoomMessage = dispelRoomMessage;
            DamageNoun = damageNoun;
            LearnDifficultyMultiplier = learnDifficultyMultiplier;
        }

        public Ability(AbilityKinds kind, AbilityAttribute attribute, MethodInfo methodInfo)
            : this(kind, attribute.Id, attribute.Name, attribute.Target, attribute.PulseWaitTime, attribute.Flags, attribute.CharacterWearOffMessage, attribute.ItemWearOffMessage, attribute.DispelRoomMessage, attribute.DamageNoun, attribute.LearnDifficultyMultiplier, attribute.Cooldown)
        {
            MethodInfo = methodInfo;
        }

        public AbilityKinds Kind { get; }

        public int Id { get; }

        public string Name { get; }

        public AbilityTargets Target { get; }

        public int PulseWaitTime { get; }

        public AbilityFlags AbilityFlags { get; }

        public string CharacterWearOffMessage { get; }

        public string ItemWearOffMessage { get; }

        public string DispelRoomMessage { get; }

        public string DamageNoun { get; }

        public int LearnDifficultyMultiplier { get; }

        public int Cooldown { get; }

        public MethodInfo MethodInfo { get; } // null for passive abilities
    }
}
