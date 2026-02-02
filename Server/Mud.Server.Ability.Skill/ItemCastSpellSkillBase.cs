using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Random;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Spell;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Ability.Skill;

public abstract class ItemCastSpellSkillBase<TItem> : SkillBase
    where TItem: IItem
{
    protected IItemManager ItemManager { get; }
    protected IAbilityManager AbilityManager { get; }

    protected ItemCastSpellSkillBase(ILogger<ItemCastSpellSkillBase<TItem>> logger, IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
        : base(logger, randomManager)
    {
        ItemManager = itemManager;
        AbilityManager = abilityManager;

        SpellInstances = [];
    }

    protected void CastSpells()
    {
        foreach (ISpell spell in SpellInstances)
            spell.Execute();
    }

    protected abstract string ActionWord { get; }

    protected List<ISpell> SpellInstances { get; } = default!;
    protected TItem Item { get; set; } = default!;

    protected string? SetupSpell(string spellName, int spellLevel, params ICommandParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(spellName))
            return null; // not really an error but don't continue
        var abilityDefinition = AbilityManager.Get(spellName, AbilityTypes.Spell);
        if (abilityDefinition == null)
        {
            Logger.LogError("Unknown spell '{spellName}' on item {item}.", spellName, Item.DebugName);
            return StringHelpers.SomethingGoesWrong;
        }
        var spellInstance = AbilityManager.CreateInstance<ISpell>(abilityDefinition.Name);
        if (spellInstance == null)
        {
            Logger.LogError("Spell '{spellName}' on item {item} cannot be instantiated.", spellName, Item.DebugName);
            return StringHelpers.SomethingGoesWrong;
        }
        var spellActionInput = new SpellActionInput(abilityDefinition, User, spellLevel, new CastFromItemOptions { Item = Item }, parameters);
        var spellInstanceGuards = spellInstance.Setup(spellActionInput);
        if (spellInstanceGuards != null)
            return spellInstanceGuards;
        SpellInstances.Add(spellInstance);
        return null;
    }

    protected string? SetupSpellForEachAvailableTargets(string spellName, int spellLevel, params ICommandParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(spellName))
            return null; // not really an error but don't continue
        var abilityDefinition = AbilityManager.Get(spellName, AbilityTypes.Spell);
        if (abilityDefinition == null)
        {
            Logger.LogError("Unknown spell '{spellName}' on item {item}.", spellName, Item.DebugName);
            return StringHelpers.SomethingGoesWrong;
        }
        if (AbilityManager.CreateInstance<ISpell>(abilityDefinition.Name) is not ITargetedAction getTargetedAction)
        {
            Logger.LogError("Spell '{spellName}' on item {item} cannot be instantiated or is not a targeted action.", spellName, Item.DebugName);
            return StringHelpers.SomethingGoesWrong;
        }

        bool atLeastOneCorrect = false;
        IEnumerable<IEntity> predefinedTargets = getTargetedAction.ValidTargets(User);
        foreach (var predefinedTarget in predefinedTargets)
        {
            var spellInstance = AbilityManager.CreateInstance<ISpell>(abilityDefinition.Name);
            if (spellInstance != null)
            {
                var spellActionInput = new SpellActionInput(abilityDefinition, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = predefinedTarget }, parameters);
                var spellInstanceGuards = spellInstance.Setup(spellActionInput);
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
        return StringHelpers.SomethingGoesWrong;
    }

    protected string? SetupSpellAndPredefinedTarget(string spellName, int spellLevel, out IEntity target, params ICommandParameter[] parameters)
    {
        target = default!;
        if (string.IsNullOrWhiteSpace(spellName))
            return null; // not really an error but don't continue
        var abilityDefinition = AbilityManager.Get(spellName, AbilityTypes.Spell);
        if (abilityDefinition == null)
        {
            Logger.LogError("Unknown spell '{spellName}' on item {item}.", spellName, Item.DebugName);
            return StringHelpers.SomethingGoesWrong;
        }

        var spellInstance = AbilityManager.CreateInstance<ISpell>(abilityDefinition.Name);
        if (spellInstance == null)
        {
            Logger.LogError("Cannot create instance of spell '{spellName}' on item {item}.", spellName, Item.DebugName);
            return StringHelpers.SomethingGoesWrong;
        }

        SpellActionInput spellActionInput;
        // no target specified
        if (parameters.Length == 0)
        {
            spellActionInput = new SpellActionInput(abilityDefinition, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = null }, parameters);
        }
        else
        {
            if (spellInstance is not ITargetedAction getTargetedAction)
            {
                spellActionInput = new SpellActionInput(abilityDefinition, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = null }, parameters);
            }
            else
            {
                IEnumerable<IEntity> predefinedTargets = getTargetedAction.ValidTargets(User);
                if (parameters.Length == 0)
                {
                    target = User.Fighting!;
                    if (target == null)
                        return $"{ActionWord.ToPascalCase()} whom or what?";
                }
                else
                    target = FindHelpers.FindByName(predefinedTargets, parameters[0])!;
                if (target == null)
                    return "You can't find it.";

                spellActionInput = new SpellActionInput(abilityDefinition, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = target }, parameters);
            }
        }
        var spellInstanceGuards = spellInstance.Setup(spellActionInput);
        if (spellInstanceGuards != null)
            return spellInstanceGuards;
        SpellInstances.Add(spellInstance);

        return null;
    }
}
