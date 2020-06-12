﻿using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("You are no longer invisible.")]
    [AbilityItemWearOffMessage("{0} fades into view.")]
    [AbilityDispellable("{0:N} fades into existence.")]
    public class Invisibility : ItemOrDefensiveSpellBase
    {
        public const string SpellName = "Invisibility";

        private IAuraManager AuraManager { get; }

        public Invisibility(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke(ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Invisible))
                return;

            victim.Act(ActOptions.ToAll, "{0:N} fade{0:v} out of existence.", victim);
            AuraManager.AddAura(victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level + 12), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags.Invisible, Operator = AffectOperators.Or });
        }

        protected override void Invoke(IItem item)
        {
            if (item.ItemFlags.HasFlag(ItemFlags.Invis))
            {
                Caster.Act(ActOptions.ToCharacter, "{0} is already invisible.", item);
                return;
            }

            Caster.Act(ActOptions.ToAll, "{0} fades out of sight.", item);
            AuraManager.AddAura(item, SpellName, Caster, Level, TimeSpan.FromMinutes(Level + 12), AuraFlags.None, true,
                new ItemFlagsAffect { Modifier = ItemFlags.Invis, Operator = AffectOperators.Or });
            return;
        }
    }
}
