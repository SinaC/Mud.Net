using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Continual Light", AbilityEffects.Creation | AbilityEffects.Buff)]
    public class ContinualLight : OptionalItemInventorySpellBase
    {
        private IAuraManager AuraManager { get; }
        private IItemManager ItemManager { get; }
        private ISettings Settings { get; }
        public ContinualLight(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager, IItemManager itemManager, ISettings settings)
            : base(randomManager, wiznet)
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

                AuraManager.AddAura(Item, AbilityInfo.Name, Caster, Level, Pulse.Infinite, AuraFlags.Permanent | AuraFlags.NoDispel, true,
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
