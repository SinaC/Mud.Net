using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class DetectPoison : ItemInventorySpellBase<IItemPoisonable>
    {
        public override int Id => 36;
        public override string Name => "Detect Poison";
        public override AbilityEffects Effects => AbilityEffects.Detection;
        protected override string InvalidItemTypeMsg => "It doesn't look poisoned.";

        public DetectPoison(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        public override void Action(ICharacter caster, int level, IItemPoisonable item)
        {
            if (item.IsPoisoned)
                caster.Send("You smell poisonous fumes.");
            else
                caster.Send("It looks delicious.");
        }
    }
}
