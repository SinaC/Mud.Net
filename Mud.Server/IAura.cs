using System;
using System.Collections.Generic;
using System.Text;
using Mud.Domain;

namespace Mud.Server
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

        void Update(int level, TimeSpan duration);

        T AddOrUpdateAffect<T>(Func<T, bool> filterFunc, Func<T> createFunc, Action<T> updateFunc)
            where T : IAffect;

        bool DecreasePulseLeft(int pulseCount); // return true if timed out

        void DecreaseLevel();

        void OnRemoved(); // set IsValid, Ability, Source

        // TODO: search affect method
        // TODO: method to 'merge' affect

        // Display
        void Append(StringBuilder sb);

        // Serialization
        AuraData MapAuraData();
    }
}
