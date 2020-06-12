using Mud.Common;
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
    [Spell(SpellName, AbilityEffects.Enchantment)]
    [AbilityItemWearOffMessage("{0:N}'s protective aura fades.")]
    public class Fireproof : ItemInventorySpellBase
    {
        public const string SpellName = "Fireproof";

        private IAuraManager AuraManager { get; }

        public Fireproof(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Item.ItemFlags.HasFlag(ItemFlags.BurnProof))
            {
                Caster.Act(ActOptions.ToCharacter, "{0:N} is already protected from burning.", Item);
                return;
            }

            int duration = RandomManager.Fuzzy(Level / 4);
            AuraManager.AddAura(Item, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new ItemFlagsAffect { Modifier = ItemFlags.BurnProof, Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToCharacter, "You protect {0:N} from fire.", Item);
            Caster.Act(ActOptions.ToRoom, "{0:N} is surrounded by a protective aura.", Item);
        }
    }
}
