using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public abstract class ItemCastSpellsNoChargeBase<T> : ItemBase<T>, IItemCastSpellsNoCharge
        where T : ItemCastSpellsNoChargeBlueprintBase
    {
        protected ItemCastSpellsNoChargeBase(Guid guid, T blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            SpellLevel = blueprint.SpellLevel;
            FirstSpell = GetSpell(blueprint.Spell1);
            SecondSpell = GetSpell(blueprint.Spell2);
            ThirdSpell = GetSpell(blueprint.Spell3);
            FourthSpell = GetSpell(blueprint.Spell4);
        }

        protected ItemCastSpellsNoChargeBase(Guid guid, T blueprint, ItemData data, IContainer containedInto)
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
                Log.Default.WriteLine(LogLevels.Error, "{0}.GetSpell: unknown spell {1} for blueprint id {2}", GetType().Name, name, Blueprint.Id);
            else if (ability.Kind != AbilityKinds.Spell)
                Log.Default.WriteLine(LogLevels.Error, "{0}.GetSpell: ability {1} is not a spell for blueprint id {2}", GetType().Name, name, Blueprint.Id);
            else if (ability.Target != AbilityTargets.CharacterOffensive
                        && ability.Target != AbilityTargets.CharacterDefensive
                        && ability.Target != AbilityTargets.CharacterSelf
                        && ability.Target != AbilityTargets.ItemHereOrCharacterOffensive
                        && ability.Target != AbilityTargets.ItemInventoryOrCharacterDefensive)
                Log.Default.WriteLine(LogLevels.Error, "{0}.GetSpell: ability {1} has invalid target {2} for blueprint id {3}", GetType().Name, name, ability.Target, Blueprint.Id);
            return ability;
        }
    }
}
