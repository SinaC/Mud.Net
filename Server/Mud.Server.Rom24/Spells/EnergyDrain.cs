using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell saps the experience points, mana, and movement points of its
target.")]
[OneLineHelp("drains experience and mana, while strengthening the caster")]
public class EnergyDrain : OffensiveSpellBase
{
    private const string SpellName = "Energy Drain";

    public EnergyDrain(ILogger<EnergyDrain> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        if (Victim != Caster)
            Caster.UpdateAlignment(-50);

        if (Victim.SavesSpell(Level, SchoolTypes.Negative))
        {
            Victim.Send("You feel a momentary chill.");
            return;
        }

        int damage;
        if (Victim.Level <= 2)
            damage = Victim[ResourceKinds.HitPoints] + 1;
        else
        {
            damage = RandomManager.Dice(1, Level);
            if (Victim is IPlayableCharacter pcVictim)
            {
                int lose = RandomManager.Range(Level / 2, 3 * Level / 2);
                pcVictim.GainExperience(-lose);
            }
            Victim.UpdateResource(ResourceKinds.Mana, -Victim[ResourceKinds.Mana] / 2); // half mana
            Victim.UpdateResource(ResourceKinds.MovePoints, -Victim[ResourceKinds.MovePoints] / 2); // half move
            Caster.Heal(Caster, damage); // drain HP to caster
        }

        Victim.Send("You feel your life slipping away!");
        Caster.Send("Wow....what a rush!");

        Victim.AbilityDamage(Caster, damage, SchoolTypes.Negative, "energy drain", true);
    }
}
