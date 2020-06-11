using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item
{
    public abstract class ItemCastSpellsNoChargeBase<TBlueprint> : ItemBase<TBlueprint, ItemData>, IItemCastSpellsNoCharge
        where TBlueprint : ItemCastSpellsNoChargeBlueprintBase
    {
        protected ItemCastSpellsNoChargeBase(Guid guid, TBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            SpellLevel = blueprint.SpellLevel;
            FirstSpell = GetSpell(blueprint.Spell1);
            SecondSpell = GetSpell(blueprint.Spell2);
            ThirdSpell = GetSpell(blueprint.Spell3);
            FourthSpell = GetSpell(blueprint.Spell4);
        }

        protected ItemCastSpellsNoChargeBase(Guid guid, TBlueprint blueprint, ItemData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
            SpellLevel = blueprint.SpellLevel;
            FirstSpell = GetSpell(blueprint.Spell1);
            SecondSpell = GetSpell(blueprint.Spell2);
            ThirdSpell = GetSpell(blueprint.Spell3);
            FourthSpell = GetSpell(blueprint.Spell4);
        }

        #region IItemCastSpellsNoCharge

        public int SpellLevel { get; }

        public IAbility FirstSpell { get; }

        public IAbility SecondSpell { get; }

        public IAbility ThirdSpell { get; }

        public IAbility FourthSpell { get; }

        #endregion

        protected virtual IAbility GetSpell(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            IAbility ability = AbilityManager[name];
            if (ability == null)
                Wiznet.Wiznet($"{GetType().Name}.GetSpell: unknown ability {name} for blueprint id {Blueprint.Id}", WiznetFlags.Bugs, AdminLevels.Implementor);
            else if (ability.Kind != AbilityKinds.Spell)
                Wiznet.Wiznet($"{GetType().Name}.GetSpell: ability {name} is not a spell for blueprint id {Blueprint.Id}", WiznetFlags.Bugs, AdminLevels.Implementor);
            else if (ability.Target != AbilityTargets.CharacterOffensive
                        && ability.Target != AbilityTargets.CharacterDefensive
                        && ability.Target != AbilityTargets.CharacterSelf
                        && ability.Target != AbilityTargets.ItemHereOrCharacterOffensive
                        && ability.Target != AbilityTargets.ItemInventoryOrCharacterDefensive
                        && ability.Target != AbilityTargets.None)
                Wiznet.Wiznet($"{GetType().Name}.GetSpell: ability {name} has invalid target {ability.Target} for blueprint id {Blueprint.Id}", WiznetFlags.Bugs, AdminLevels.Implementor);
            return ability;
        }
    }
}
