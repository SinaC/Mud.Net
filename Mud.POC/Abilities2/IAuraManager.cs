using Mud.POC.Abilities2.Domain;
using System;
using Mud.POC.Abilities2.ExistingCode;

namespace Mud.POC.Abilities2
{
    public interface IAuraManager
    {
        IAura AddAura(IEntity target, string abilityName, ICharacter source, int level, TimeSpan duration, AuraFlags flags, bool recompute, params IAffect[] affects);
    }
}
