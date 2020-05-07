using Mud.Domain;
using System.Collections.Generic;

namespace Mud.Server.Classes
{
    public class Mage : ClassBase
    {
        #region IClass

        public override string Name => "mage";

        public override string ShortName => "Mag";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Mana
        };

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            return ResourceKinds; // always mana
        }

        public override int MaxPracticePercentage => 75;

        public override int GetAttributeByLevel(CharacterAttributes attribute, int level)
        {
            return level * 10; // TODO: http://wow.gamepedia.com/Base_attributes
        }

        #endregion

        public Mage()
        {
            AddAbility(2, "Detect Magic", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(3, "Detect Invis", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(4, "Chill Touch", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
            AddAbility(6, "Continual Light", Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 1);
            AddAbility(6, "Faerie Fire", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(7, "Armor", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(7, "Burning Hands", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
            AddAbility(8, "Create Water", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(10, "Create Food", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(11, "Detect Evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(11, "Detect Good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(12, "Blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(14, "Create Spring", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
            AddAbility(14, "Faerie Fog", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
            AddAbility(15, "Control Weather", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
            AddAbility(15, "Detect Hidden", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(15, "Detect Poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(16, "Colour Spray", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
            AddAbility(16, "Create Rose", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
            AddAbility(16, "Dispel Magic", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
            AddAbility(16, "Enchant Armor", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
            AddAbility(17, "Enchant Weapon", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
            AddAbility(18, "Cancellation", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
            AddAbility(18, "Curse", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
            AddAbility(19, "Energy Drain", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
            AddAbility(20, "Charm Person", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
            AddAbility(26, "Call Ligthning", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
            AddAbility(28, "Acid Blast", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
            AddAbility(33, "Chain Lightning", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
            AddAbility(48, "Calm", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);

            //TODO: continue after Faerie Fog
            //AddAbility(14, "Faerie Fog", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        }
    }
}
