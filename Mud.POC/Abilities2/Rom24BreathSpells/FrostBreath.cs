﻿using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Effects;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24BreathSpells
{
    [Spell(SpellName, AbilityEffects.DamageArea | AbilityEffects.Debuff, PulseWaitTime = 24)]
    public class FrostBreath : OffensiveSpellBase
    {
        public const string SpellName = "Frost Breath";

        private IAuraManager AuraManager { get; }
        private IItemManager ItemManager { get; }

        public FrostBreath(IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
            ItemManager = itemManager;
        }

        protected override void Invoke()
        {
            Caster.ActToNotVictim(Victim, "{0} breathes out a freezing cone of frost!", Caster);
            Victim.Act(ActOptions.ToCharacter, "{0} breathes a freezing cone of frost over you!", Caster);
            Caster.Send("You breath out a cone of frost.");

            int hp = Math.Max(12, Victim.HitPoints);
            int hpDamage = RandomManager.Range(1 + hp / 11, hp / 6);
            int diceDamage = RandomManager.Dice(Level, 18);
            int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

            BreathAreaAction breathAreaAction = new BreathAreaAction();
            breathAreaAction.Apply(Victim, Caster, Level, damage, SchoolTypes.Cold, "blast of frost", ChillTouch.SpellName, () => new ColdEffect(RandomManager, AuraManager, ItemManager), () => new ColdEffect(RandomManager, AuraManager, ItemManager));
        }
    }
}
