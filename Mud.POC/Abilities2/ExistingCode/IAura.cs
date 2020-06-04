using System;
using Mud.POC.Abilities2.Domain;
using System.Collections.Generic;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IAura
    {
        bool IsValid { get; } // auras are not removed immediately but in cleanup step

        int Level { get; }

        int PulseLeft { get; } // irrelevant if AuraFlags.Permanent is set

        string AbilityName { get; }

        IEntity Source { get; }

        AuraFlags AuraFlags { get; }

        IEnumerable<IAffect> Affects { get; } // affects linked to this aura

        void Update(int level, TimeSpan duration);

        T AddOrUpdateAffect<T>(Func<T, bool> filterFunc, Func<T> createFunc, Action<T> updateFunc)
            where T : IAffect;

        int DecreaseLevel();
    }
}
