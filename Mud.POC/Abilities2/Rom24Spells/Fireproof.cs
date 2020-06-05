using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Fireproof", AbilityEffects.Enchantment)]
    [AbilityItemWearOffMessage("{0:N}'s protective aura fades.")]
    public class Fireproof : ItemInventorySpellBase
    {
        private IAuraManager AuraManager { get; }

        public Fireproof(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
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
            AuraManager.AddAura(Item, AbilityInfo.Name, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new ItemFlagsAffect { Modifier = ItemFlags.BurnProof, Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToCharacter, "You protect {0:N} from fire.", Item);
            Caster.Act(ActOptions.ToRoom, "{0:N} is surrounded by a protective aura.", Item);
        }
    }
}
