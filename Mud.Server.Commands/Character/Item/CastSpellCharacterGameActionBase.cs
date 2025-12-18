using Microsoft.Extensions.Logging;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

public abstract class CastSpellCharacterGameActionBase : CharacterGameAction
{
    private ILogger<CastSpellCharacterGameActionBase> Logger { get; }
    private ICommandParser CommandParser { get; }
    private IAbilityManager AbilityManager { get; }

    protected CastSpellCharacterGameActionBase(ILogger<CastSpellCharacterGameActionBase> logger, ICommandParser commandParser, IAbilityManager abilityManager)
    {
        Logger = logger;
        CommandParser = commandParser;
        AbilityManager = abilityManager;
    }

    protected string? CastSpell(IItem item, string spellName, int spellLevel)
    {
        if (string.IsNullOrWhiteSpace(spellName))
            return null; // not really an error but don't continue
        var abilityDefinition = AbilityManager.Search(spellName, AbilityTypes.Spell);
        if (abilityDefinition == null)
        {
            Logger.LogError("Unknown spell '{spellName}' on item {item}.", spellName, item.DebugName);
            return "Something goes wrong.";
        }
        var spellInstance = AbilityManager.CreateInstance<ISpell>(abilityDefinition.Name);
        if (spellInstance == null)
        {
            Logger.LogError("Spell '{spellName}' on item {item} cannot be instantiated.", spellName, item.DebugName);
            return "Something goes wrong.";
        }
        var spellActionInput = new SpellActionInput(abilityDefinition, Actor, spellLevel, new CastFromItemOptions { Item = item }, CommandParser.NoParameters);
        var spellInstanceGuards = spellInstance.Setup(spellActionInput);
        if (spellInstanceGuards != null)
            return spellInstanceGuards;
        spellInstance.Execute();
        return null;
    }
}
