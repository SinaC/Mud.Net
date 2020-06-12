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
    [Spell(SpellName, AbilityEffects.Creation | AbilityEffects.Buff)]
    public class ContinualLight : OptionalItemInventorySpellBase
    {
        public const string SpellName = "Continual Light";

        private IAuraManager AuraManager { get; }
        private IItemManager ItemManager { get; }
        private ISettings Settings { get; }

        public ContinualLight(IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager, ISettings settings)
            : base(randomManager)
        {
            AuraManager = auraManager;
            ItemManager = itemManager;
            Settings = settings;
        }

        protected override void Invoke()
        {
            if (Item != null)
            {
                if (Item.ItemFlags.HasFlag(ItemFlags.Glowing))
                {
                    Caster.Act(ActOptions.ToCharacter, "{0} is already glowing.", Item);
                    return;
                }

                AuraManager.AddAura(Item, SpellName, Caster, Level, Pulse.Infinite, AuraFlags.Permanent | AuraFlags.NoDispel, true,
                    new ItemFlagsAffect { Modifier = ItemFlags.Glowing, Operator = AffectOperators.Or });
                Caster.Act(ActOptions.ToAll, "{0} glows with a white light.", Item);
                return;
            }
            // create item
            IItem light = ItemManager.AddItem(Guid.NewGuid(), Settings.LightBallBlueprintId, Caster.Room);
            Caster.Act(ActOptions.ToAll, "{0} twiddle{0:v} {0:s} thumbs and {1} appears.", Caster, light);
        }
    }
}
