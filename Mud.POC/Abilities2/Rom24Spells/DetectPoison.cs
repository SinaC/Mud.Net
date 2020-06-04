using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Detect Poison", AbilityEffects.Detection)]
    public class DetectPoison : ItemInventorySpellBase<IItemPoisonable>
    {
        protected override string InvalidItemTypeMsg => "It doesn't look poisoned.";

        public DetectPoison(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (Item.IsPoisoned)
                Caster.Send("You smell poisonous fumes.");
            else
                Caster.Send("It looks delicious.");
        }
    }
}
