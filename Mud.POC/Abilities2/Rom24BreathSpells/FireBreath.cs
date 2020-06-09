using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Effects;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24BreathSpells
{
    [Spell(SpellName, AbilityEffects.DamageArea | AbilityEffects.Debuff, PulseWaitTime = 24)]
    public class FireBreath : OffensiveSpellBase
    {
        public const string SpellName = "Fire Breath";

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

            BreathAreaAction breathAreaAction = new BreathAreaAction();
            breathAreaAction.Apply(Victim, Caster, Level, damage, SchoolTypes.Fire, "blast of fire", SpellName, () => new FireEffect(RandomManager, AuraManager, ItemManager), () => new FireEffect(RandomManager, AuraManager, ItemManager));
        }
    }
}
