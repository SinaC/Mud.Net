using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class ChainLightning : OffensiveSpellBase
    {
        public override int Id => 12;
        public override string Name => "Chain Lightning";

        public override AbilityEffects Effects => AbilityEffects.DamageArea;

        public ChainLightning(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            caster.Act(ActOptions.ToRoom, "A lightning bolt leaps from {0}'s hand and arcs to {1}.", caster, victim);
            caster.Act(ActOptions.ToCharacter, "A lightning bolt leaps from your hand and arcs to {0}.", victim);
            victim.Act(ActOptions.ToCharacter, "A lightning bolt leaps from {0}'s hand and hits you!", caster);

            int damage = RandomManager.Dice(level, 6);
            if (victim.SavesSpell(level, SchoolTypes.Lightning))
                damage /= 3;
            victim.AbilityDamage(caster, this, damage, SchoolTypes.Lightning, "lightning", true);

            // Hops from one victim to another
            ICharacter lastVictim = victim;
            level -= 4; // decrement damage
            while (level > 0)
            {
                // search a new victim
                ICharacter target = caster.Room.People.FirstOrDefault(x => x != lastVictim && victim.IsSafeSpell(caster, true));
                if (target != null) // target found
                {
                    target.Act(ActOptions.ToRoom, "The bolt arcs to {0}!", target);
                    target.Send("The bolt hits you!");
                }
                else // no target found, hits caster
                {
                    if (lastVictim == caster) // no double hits
                    {
                        caster.Act(ActOptions.ToRoom, "The bolt seems to have fizzled out.");
                        caster.Send("The bolt grounds out through your body.");
                        return;
                    }
                    caster.Act(ActOptions.ToRoom, "The bolt arcs to {0}...whoops!", caster);
                    caster.Send("You are struck by your own lightning!");
                }
                damage = RandomManager.Dice(level, 6);
                if (victim.SavesSpell(level, SchoolTypes.Lightning))
                    damage /= 3;
                victim.AbilityDamage(caster, this, damage, SchoolTypes.Lightning, "lightning", true);
                level -= 4; // decrement damage
                lastVictim = target;
            }
        }
    }
}
