using Mud.Container;
using Mud.Logger;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;
using System.Collections.Generic;

namespace Mud.POC.Abilities2
{
    public abstract class ItemCastSpellSkillBase<TItem> : SkillBase
        where TItem: IItem
    {
        protected TItem Item { get; set; }

        protected IAbilityManager AbilityManager { get; }
        protected IItemManager ItemManager { get; }
        protected List<ISpell> SpellInstances { get; }

        protected ItemCastSpellSkillBase(IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
            : base(randomManager)
        {
            AbilityManager = abilityManager;
            ItemManager = itemManager;
            SpellInstances = new List<ISpell>();
        }

        protected void CastSpells()
        {
            foreach (ISpell spell in SpellInstances)
                spell.Execute();
        }

        protected string SetSpellTargets(string spellName, int spellLevel, string rawParameters, params CommandParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(spellName))
                return null; // not really an error but don't continue
            var abilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Unknown spell '{0}' on item {1}.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }
            if (DependencyContainer.Current.GetRegistration(abilityInfo.AbilityExecutionType, false) == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Spell '{0}' on item {1} has been not found in DependencyContainer.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }
            var spellInstance = (ISpell)DependencyContainer.Current.GetInstance(abilityInfo.AbilityExecutionType);
            if (spellInstance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Spell '{0}' on item {1} cannot be instantiated.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }
            var spellActionInput = new SpellActionInput(abilityInfo, User, spellLevel, true, rawParameters, parameters);
            string spellInstanceGuards = spellInstance.Setup(spellActionInput);
            if (spellInstanceGuards != null)
                return spellInstanceGuards;
            SpellInstances.Add(spellInstance);
            return null;
        }
    }
}
