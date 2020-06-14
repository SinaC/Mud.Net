﻿using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("The pink aura around you fades away.")]
    [AbilityDispellable("{0:N}'s outline fades.")]
    public class FaerieFire : OffensiveSpellBase
    {
        public const string SpellName = "Faerie Fire";

        private IAuraManager AuraManager { get; }

        public FaerieFire(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.FaerieFire))
                return;
            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 2 * Level, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.FaerieFire, Operator = AffectOperators.Or });
            Victim.Act(ActOptions.ToAll, "{0:N} are surrounded by a pink outline.", Victim);
        }
    }
}
