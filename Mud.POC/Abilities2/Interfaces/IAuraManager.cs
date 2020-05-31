using Mud.POC.Abilities2.Domain;
using System;

namespace Mud.POC.Abilities2.Interfaces
{
    public interface IAuraManager
    {
        IAura AddAura(IEntity target, IAbility ability, ICharacter source, int level, TimeSpan duration, AuraFlags flags, bool recompute, params IAffect[] affects);
    }
}
