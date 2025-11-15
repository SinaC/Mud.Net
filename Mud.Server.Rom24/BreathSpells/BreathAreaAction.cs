using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Room;
using System.Collections.ObjectModel;

namespace Mud.Server.Rom24.BreathSpells;

public class BreathAreaAction
{
    public void Apply(ICharacter victim, ICharacter caster, int level, int damage, SchoolTypes damageType, string damageNoun, string auraName, Func<IEffect<IRoom>> roomEffectFunc, Func<IEffect<ICharacter>> characterEffectFunc)
    {
        // Room
        roomEffectFunc().Apply(caster.Room,  caster, auraName, level, damage / 2);
        // Room people
        var clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.Where(x =>
            !(x.IsSafeSpell(caster, true) 
              || (x is INonPlayableCharacter && caster is INonPlayableCharacter && (caster.Fighting != x || x.Fighting != caster)))).ToList());
        foreach (ICharacter coVictim in clone)
        {
            if (victim == coVictim) // full damage
            {
                if (coVictim.SavesSpell(level, damageType))
                {
                    characterEffectFunc().Apply(coVictim, caster, auraName, level / 2, damage / 4);
                    coVictim.AbilityDamage(caster, damage / 2, damageType, damageNoun, true);
                }
                else
                {
                    characterEffectFunc().Apply(coVictim, caster, auraName, level, damage);
                    coVictim.AbilityDamage(caster, damage, damageType, damageNoun, true);
                }
            }
            else // partial damage
            {
                if (coVictim.SavesSpell(level - 2, damageType))
                {
                    characterEffectFunc().Apply(coVictim, caster, auraName, level / 4, damage / 8);
                    coVictim.AbilityDamage(caster, damage / 2, damageType, damageNoun, true);
                }
                else
                {
                    characterEffectFunc().Apply(coVictim, caster, auraName, level / 2, damage / 4);
                    coVictim.AbilityDamage(caster, damage / 2, damageType, damageNoun, true);
                }
            }
        }
    }
}
