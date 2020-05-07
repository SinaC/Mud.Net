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

        // Display
        void Append(StringBuilder sb);

        // Serialization
        AuraData MapAuraData();
    }
}
