using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.DamageArea)]
[Syntax("cast [spell] <target>")]
[Help(
@"Chain lightning is a deadly spell, producing a powerful bolt of lightning
that arcs from target to target in the room, until its force is fully
expended.  Allies of the caster may be hit by this spell if they are members 
of a clan, while the caster himself will not be struck unless no other
viable target remains.  Chain lightning is most effective when used on
groups of creatures.")]
[OneLineHelp("sends lightning bolts arcing through foes")]
public class ChainLightning : OffensiveSpellBase
{
    private const string SpellName = "Chain Lightning";

    public ChainLightning(ILogger<ChainLightning> logger, IRandomManager randomManager)
        : base(logger, randomManager)
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
            var target = Caster.Room.People.FirstOrDefault(x => x != lastVictim && Victim.IsSafeSpell(Caster, true));
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
