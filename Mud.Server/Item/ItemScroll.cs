using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public class ItemScroll : ItemCastSpellsNoRechargeBase<ItemScrollBlueprint>, IItemScroll
    {
        public ItemScroll(Guid guid, ItemScrollBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemScroll(Guid guid, ItemScrollBlueprint blueprint, ItemData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
        }

        protected override IAbility GetSpell(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            IAbility ability = AbilityManager[name];
            if (ability == null)
                Log.Default.WriteLine(LogLevels.Error, "ItemScroll.GetSpell: unknown spell {0} for blueprint id {1}", name, Blueprint.Id);
            else if (ability.Kind != AbilityKinds.Spell)
                Log.Default.WriteLine(LogLevels.Error, "ItemScroll.GetSpell: ability {0} is not a spell for blueprint id {1}", name, Blueprint.Id);
            // No target check
            return ability;
        }
    }
}
