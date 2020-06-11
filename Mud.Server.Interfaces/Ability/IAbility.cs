using System.Reflection;
using Mud.Domain;

namespace Mud.Server.Interfaces.Ability
{
    public interface IAbility
    {
        AbilityKinds Kind { get; }

        int Id { get; }

        string Name { get; }

        AbilityTargets Target { get; }

        int PulseWaitTime { get; }

        AbilityFlags AbilityFlags { get; }

        string CharacterWearOffMessage { get; }

        string ItemWearOffMessage { get; }

        string DispelRoomMessage { get; }

        string DamageNoun { get; }

        int LearnDifficultyMultiplier { get; }

        int Cooldown { get; }

        MethodInfo MethodInfo { get; } // null for passive abilities
    }
}
