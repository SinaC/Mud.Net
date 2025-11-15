using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.BreathSpells;

[Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff)]
public class LightningBreath : OffensiveSpellBase
{
    private const string SpellName = "Lightning Breath";

    private IItemManager ItemManager { get; }

    public LightningBreath(IRandomManager randomManager, IItemManager itemManager)
        : base(randomManager)
    {
        ItemManager = itemManager;
    }

    protected override void Invoke()
    {
        Caster.ActToNotVictim(Victim, "{0} breathes a bolt of lightning at {1}.", Caster, Victim);
        Victim.Act(ActOptions.ToCharacter, "{0} breathes a bolt of lightning at you!", Caster);
        Caster.Act(ActOptions.ToCharacter, "You breathe a bolt of lightning at {0}.", Victim);

        int hp = Math.Max(10, Victim.HitPoints);
        int hpDamage = RandomManager.Range(1 + hp / 9, hp / 5);
        int diceDamage = RandomManager.Dice(Level, 20);
        int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

        if (Victim.SavesSpell(Level, SchoolTypes.Lightning))
        {
            var victimShockEffect = new ShockEffect(RandomManager, ItemManager);
            victimShockEffect.Apply(Victim, Caster, SpellName, Level / 2, damage / 4);
            Victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Lightning, "blast of lightning", true);
        }
        else
        {
            var victimShockEffect = new ShockEffect(RandomManager, ItemManager);
            victimShockEffect.Apply(Victim, Caster, SpellName, Level, damage);
            Victim.AbilityDamage(Caster, damage, SchoolTypes.Lightning, "blast of lightning", true);
        }
    }
}
