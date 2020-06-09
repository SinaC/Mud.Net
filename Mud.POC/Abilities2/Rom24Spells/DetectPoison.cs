using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Detection)]
    public class DetectPoison : ItemInventorySpellBase<IItemPoisonable>
    {
        public const string SpellName = "Detect Poison";

        public DetectPoison(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string InvalidItemTypeMsg => "It doesn't look poisoned.";

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
