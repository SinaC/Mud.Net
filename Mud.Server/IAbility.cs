using System.Reflection;
using Mud.Domain;

namespace Mud.Server
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

        int LearnDifficultyMultiplier { get; }

        MethodInfo MethodInfo { get; } // null for passive abilities
    }
}
