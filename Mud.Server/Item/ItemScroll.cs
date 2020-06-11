using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item
{
    public class ItemScroll : ItemCastSpellsNoChargeBase<ItemScrollBlueprint>, IItemScroll
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
                Wiznet.Wiznet($"ItemScroll.GetSpell: unknown ability {name} for blueprint id {Blueprint.Id}", WiznetFlags.Bugs, AdminLevels.Implementor);
            else if (ability.Kind != AbilityKinds.Spell)
                Wiznet.Wiznet($"ItemScroll.GetSpell: ability {name} is not a spell for blueprint id {Blueprint.Id}", WiznetFlags.Bugs, AdminLevels.Implementor);
            // No target check
            return ability;
        }
    }
}
