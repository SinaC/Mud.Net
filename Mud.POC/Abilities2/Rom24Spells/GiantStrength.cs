﻿using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("You feel weaker.")]
    [AbilityDispellable("{0:N} no longer looks so mighty.")]
    public class GiantStrength : DefensiveSpellBase
    {
        public const string SpellName = "Giant Strength";

        private IAuraManager AuraManager { get; }

        public GiantStrength(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.GetAura(SpellName) != null)
            {
                if (Victim == Caster)
                    Caster.Send("You are already as strong as you can get!");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} can't get any stronger.", Victim);
                return;
            }
            int modifier = 1 + (Level >= 18 ? 1 : 0) + (Level >= 25 ? 1 : 0) + (Level >= 32 ? 1 : 0);
            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = modifier, Operator = AffectOperators.Add });
            Victim.Act(ActOptions.ToAll, "{0:P} muscles surge with heightened power.", Victim);
        }
    }
}
