using Mud.Logger;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using System.Collections.Generic;

namespace Mud.Server.Ability.Skill
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

        protected string SetupSpell(string spellName, int spellLevel, params ICommandParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(spellName))
                return null; // not really an error but don't continue
            var abilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Unknown spell '{0}' on item {1}.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }
            var spellInstance = AbilityManager.CreateInstance<ISpell>(abilityInfo.Name);
            if (spellInstance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Spell '{0}' on item {1} cannot be instantiated.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }
            var spellActionInput = new SpellActionInput(abilityInfo, User, spellLevel, new CastFromItemOptions { Item = Item }, parameters);
            string spellInstanceGuards = spellInstance.Setup(spellActionInput);
            if (spellInstanceGuards != null)
                return spellInstanceGuards;
            SpellInstances.Add(spellInstance);
            return null;
        }

        protected string SetupSpellForEachAvailableTargets(string spellName, int spellLevel, params ICommandParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(spellName))
                return null; // not really an error but don't continue
            var abilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Unknown spell '{0}' on item {1}.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }
            var getTargetedAction = AbilityManager.CreateInstance<ISpell>(abilityInfo.Name) as ITargetedAction;
            if (getTargetedAction == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Spell '{0}' on item {1} cannot be instantiated or is not a targeted action.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }

            bool atLeastOneCorrect = false;
            IEnumerable<IEntity> predefinedTargets = getTargetedAction.ValidTargets(User);
            foreach (IEntity predefinedTarget in predefinedTargets)
            {
                var spellInstance = AbilityManager.CreateInstance<ISpell>(abilityInfo.Name);
                if (spellInstance != null)
                {
                    var spellActionInput = new SpellActionInput(abilityInfo, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = predefinedTarget }, parameters);
                    string spellInstanceGuards = spellInstance.Setup(spellActionInput);
                    if (spellInstanceGuards != null)
                        User.Send(spellInstanceGuards); // not usual way Setup/Guards
                    else
                    {
                        atLeastOneCorrect = true;
                        SpellInstances.Add(spellInstance);
                    }
                }
            }
            if (atLeastOneCorrect)
                return null;
            return "Something goes wrong";
        }

        protected string SetupSpellAndPredefinedTarget(string spellName, int spellLevel, out IEntity target, params ICommandParameter[] parameters)
        {
            target = null;
            if (string.IsNullOrWhiteSpace(spellName))
                return null; // not really an error but don't continue
            var abilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Unknown spell '{0}' on item {1}.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }

            var spellInstance = AbilityManager.CreateInstance<ISpell>(abilityInfo.Name);
            if (spellInstance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot create instance of spell '{0}' on item {1}.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }

            SpellActionInput spellActionInput;
            // no target specified
            if (parameters.Length == 0)
            {
                spellActionInput = new SpellActionInput(abilityInfo, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = null }, parameters);
            }
            else
            {
                var getTargetedAction = spellInstance as ITargetedAction;
                if (getTargetedAction == null)
                {
                    spellActionInput = new SpellActionInput(abilityInfo, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = null }, parameters);
                }
                else
                {
                    IEnumerable<IEntity> predefinedTargets = getTargetedAction.ValidTargets(User);
                    if (parameters.Length == 0)
                    {
                        target = User.Fighting;
                        if (target == null)
                            return "Zap whom or what?";
                    }
                    else
                        target = FindHelpers.FindByName(predefinedTargets, parameters[0]);
                    if (target == null)
                        return "You can't find it.";

                    spellActionInput = new SpellActionInput(abilityInfo, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = target }, parameters);
                }
            }
            string spellInstanceGuards = spellInstance.Setup(spellActionInput);
            if (spellInstanceGuards != null)
                return spellInstanceGuards;
            SpellInstances.Add(spellInstance);

            return null;
        }
    }
}
