using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item
{
    public abstract class ItemCastSpellsChargeBase<TBlueprint, TData> : ItemBase<TBlueprint, TData>, IItemCastSpellsCharge
        where TBlueprint : ItemCastSpellsChargeBlueprintBase
        where TData: ItemCastSpellsChargeData
    {
        protected ItemCastSpellsChargeBase(Guid guid, TBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            SpellLevel = blueprint.SpellLevel;
            MaxChargeCount = blueprint.MaxChargeCount;
            CurrentChargeCount = blueprint.CurrentChargeCount;
            Spell = GetSpell(blueprint.Spell);
            AlreadyRecharged = blueprint.AlreadyRecharged;
        }

        protected ItemCastSpellsChargeBase(Guid guid, TBlueprint blueprint, TData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
            SpellLevel = blueprint.SpellLevel;
            MaxChargeCount = data.MaxChargeCount;
            CurrentChargeCount = data.CurrentChargeCount;
            Spell = GetSpell(blueprint.Spell);
            AlreadyRecharged = data.AlreadyRecharged;
        }

        #region IItemCastSpellsCharge

        public int SpellLevel { get; }
        public int CurrentChargeCount { get; protected set; }
        public int MaxChargeCount { get; protected set; }
        public IAbility Spell { get; }
        public bool AlreadyRecharged { get; protected set; }

        public void Use()
        {
            CurrentChargeCount = Math.Max(0, CurrentChargeCount - 1);
        }

        public void Recharge(int currentChargeCount, int maxChargeCount)
        {
            AlreadyRecharged = true;
            MaxChargeCount = maxChargeCount;
            CurrentChargeCount = Math.Min(currentChargeCount, MaxChargeCount);
        }

        #endregion

        protected IAbility GetSpell(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            IAbility ability = AbilityManager[name];
            if (ability == null)
                Wiznet.Wiznet($"{GetType().Name}.GetSpell: unknown ability {name} for blueprint id {Blueprint.Id}", WiznetFlags.Bugs, AdminLevels.Implementor);
            else if (ability.Kind != AbilityKinds.Spell)
                Wiznet.Wiznet($"{GetType().Name}.GetSpell: ability {name} is not a spell for blueprint id {Blueprint.Id}", WiznetFlags.Bugs, AdminLevels.Implementor);
            return ability;
        }
    }
}
