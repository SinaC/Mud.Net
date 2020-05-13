using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using System;

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
                Log.Default.WriteLine(LogLevels.Error, "{0}.GetSpell: unknown spell {1} for blueprint id {2}", GetType().Name, name, Blueprint.Id);
            else if (ability.Kind != AbilityKinds.Spell)
                Log.Default.WriteLine(LogLevels.Error, "{0}.GetSpell: ability {1} is not a spell for blueprint id {2}", GetType().Name, name, Blueprint.Id);
            return ability;
        }
    }
}
