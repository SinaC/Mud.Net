using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.DamageArea)]
    public class ChainLightning : OffensiveSpellBase
    {
        public const string SpellName = "Chain Lightning";

        public ChainLightning(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            Caster.Act(ActOptions.ToRoom, "A lightning bolt leaps from {0}'s hand and arcs to {1}.", Caster, Victim);
            Caster.Act(ActOptions.ToCharacter, "A lightning bolt leaps from your hand and arcs to {0}.", Victim);
            Victim.Act(ActOptions.ToCharacter, "A lightning bolt leaps from {0}'s hand and hits you!", Caster);

            int level = Level; // will decrease on each hop
            int damage = RandomManager.Dice(level, 6);
            if (Victim.SavesSpell(level, SchoolTypes.Lightning))
                damage /= 3;
            Victim.AbilityDamage(Caster, damage, SchoolTypes.Lightning, "lightning", true);

            // hops from one victim to another
            ICharacter lastVictim = Victim;
            level -= 4; // decrement damage
            while (level > 0)
            {
                // search a new Victim
                ICharacter target = Caster.Room.People.FirstOrDefault(x => x != lastVictim && Victim.IsSafeSpell(Caster, true));
                if (target != null) // target found
                {
                    target.Act(ActOptions.ToRoom, "The bolt arcs to {0}!", target);
                    target.Send("The bolt hits you!");
                }
                else // no target found, hits Caster
                {
                    if (lastVictim == Caster) // no double hits
                    {
                        Caster.Act(ActOptions.ToRoom, "The bolt seems to have fizzled out.");
                        Caster.Send("The bolt grounds out through your body.");
                        return;
                    }
                    Caster.Act(ActOptions.ToRoom, "The bolt arcs to {0}...whoops!", Caster);
                    Caster.Send("You are struck by your own lightning!");
                }
                damage = RandomManager.Dice(level, 6);
                if (Caster.SavesSpell(level, SchoolTypes.Lightning))
                    damage /= 3;
                Caster.AbilityDamage(Caster, damage, SchoolTypes.Lightning, "lightning", true);
                level -= 4; // decrement damage
                lastVictim = Caster;
            }
        }
    }
}
