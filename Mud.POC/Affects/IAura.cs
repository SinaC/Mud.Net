using System.Collections.Generic;
using System.Text;

namespace Mud.POC.Affects
{
    public interface IAura
    {
        bool IsValid { get; } // auras are not removed immediately but in cleanup step

        int Level { get; }

        int PulseLeft { get; } // irrelevant if AuraFlags.Permanent is set

        IAbility Ability { get; }

        IEntity Source { get; }

        AuraFlags AuraFlags { get; }

        IEnumerable<IAffect> Affects { get; } // affects linked to this aura

        bool DecreasePulseLeft(int pulseCount); // return true if timed out

        void OnRemoved(); // set IsValid, Ability, Source

        void Append(StringBuilder sb);
    }

    public enum AuraFlags
    {
        StayDeath = 0, // Remains even if affected dies
        NoDispel = 1, // Can't be dispelled
        Permanent = 2, // No duration
        Hidden = 3, // Not displayed
    }

}
