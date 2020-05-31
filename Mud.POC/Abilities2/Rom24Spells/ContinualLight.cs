using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class ContinualLight : OptionalItemInventorySpellBase
    {
        public override int Id => 17;
        public override string Name => "Continual Light";
        public override AbilityEffects Effects => AbilityEffects.Creation;

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

        public override void Action(ICharacter caster, int level, IItem item)
        {
            if (item != null)
            {
                if (item.ItemFlags.HasFlag(ItemFlags.Glowing))
                {
                    caster.Act(ActOptions.ToCharacter, "{0} is already glowing.", item);
                    return;
                }

                AuraManager.AddAura(item, this, caster, level, Pulse.Infinite, AuraFlags.Permanent | AuraFlags.NoDispel, true,
                    new ItemFlagsAffect { Modifier = ItemFlags.Glowing, Operator = AffectOperators.Or });
                caster.Act(ActOptions.ToAll, "{0} glows with a white light.", item);
                return;
            }
            // create item
            IItem light = ItemManager.AddItem(Guid.NewGuid(), Settings.LightBallBlueprintId, caster.Room);
            caster.Act(ActOptions.ToAll, "{0} twiddle{0:v} {0:s} thumbs and {1} appears.", caster, light);
        }
    }
}
