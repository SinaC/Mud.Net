using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.BreathSpells;

[Spell(SpellName, AbilityEffects.DamageArea | AbilityEffects.Debuff, PulseWaitTime = 24)]
[AbilityCharacterWearOffMessage("The smoke leaves your eyes.")]
public class FireBreath : OffensiveSpellBase
{
    private const string SpellName = "Fire Breath";

    private IAuraManager AuraManager { get; }
    private IItemManager ItemManager { get; }

    public FireBreath(IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
        : base(randomManager)
    {
        AuraManager = auraManager;
        ItemManager = itemManager;
    }

    protected override void Invoke()
    {
        Caster.ActToNotVictim(Victim, "{0} breathes forth a cone of fire.", Caster);
        Victim.Act(ActOptions.ToCharacter, "{0} breathes a cone of hot fire over you!", Caster);
        Caster.Send("You breath forth a cone of fire.");

        int hp = Math.Max(10, Victim.HitPoints);
        int hpDamage = RandomManager.Range(1 + hp / 9, hp / 5);
        int diceDamage = RandomManager.Dice(Level, 20);
        int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

        BreathAreaAction breathAreaAction = new ();
        breathAreaAction.Apply(Victim, Caster, Level, damage, SchoolTypes.Fire, "blast of fire", SpellName, () => new FireEffect(RandomManager, AuraManager, ItemManager), () => new FireEffect(RandomManager, AuraManager, ItemManager));
    }
}
