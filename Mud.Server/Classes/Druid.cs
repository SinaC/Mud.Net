using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Classes
{
    public class Druid : ClassBase
    {
        private readonly List<ResourceKinds> _rageOnly = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Rage
        };

        private readonly List<ResourceKinds> _energyOnly = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Energy
        };

        private readonly List<ResourceKinds> _manaOnly = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Mana
        };

        #region IClass

        public override string Name => "druid";

        public override string ShortName => "Dru";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Mana, // others
            Domain.ResourceKinds.Energy, // cat form
            Domain.ResourceKinds.Rage // bear form
        };

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            switch (form)
            {
                case Forms.Bear:
                    return _rageOnly;
                case Forms.Cat:
                    return _energyOnly;
                default:
                    return _manaOnly;
            }
        }

        public override int MaxPracticePercentage => 75;

        public override int GetAttributeByLevel(CharacterAttributes attribute, int level)
        {
            return level*10; // TODO: http://wow.gamepedia.com/Base_attributes
        }

        #endregion

        public Druid()
        {
            // Test class with all skills + Passive
            foreach (IAbility ability in AbilityManager.Skills)
                AddAbility(20, ability, Domain.ResourceKinds.None, 0, CostAmountOperators.None, 1);
            foreach (IAbility ability in AbilityManager.Passives)
                AddAbility(10, ability, Domain.ResourceKinds.None, 0, CostAmountOperators.None, 1);
        }
    }
}
