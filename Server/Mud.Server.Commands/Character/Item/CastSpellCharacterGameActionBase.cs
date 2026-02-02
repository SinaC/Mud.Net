using Microsoft.Extensions.Logging;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Spell;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

public abstract class CastSpellCharacterGameActionBase<TCharacter, TCharacterGameActionInfo> : CharacterGameActionBase<TCharacter, TCharacterGameActionInfo>
    where TCharacter : class, ICharacter
    where TCharacterGameActionInfo : class, ICharacterGameActionInfo
{
    private ILogger<CastSpellCharacterGameActionBase<TCharacter, TCharacterGameActionInfo>> Logger { get; }
    private ICommandParser CommandParser { get; }
    private IAbilityManager AbilityManager { get; }

    protected CastSpellCharacterGameActionBase(ILogger<CastSpellCharacterGameActionBase<TCharacter, TCharacterGameActionInfo>> logger, ICommandParser commandParser, IAbilityManager abilityManager)
    {
        Logger = logger;
        CommandParser = commandParser;
        AbilityManager = abilityManager;
    }

    protected string? CastSpell(IItem item, string spellName, int spellLevel)
    {
        if (string.IsNullOrWhiteSpace(spellName))
            return null; // not really an error but don't continue
        var abilityDefinition = AbilityManager.Get(spellName, AbilityTypes.Spell);
        if (abilityDefinition == null)
        {
            Logger.LogError("Unknown spell '{spellName}' on item {item}.", spellName, item.DebugName);
            return StringHelpers.SomethingGoesWrong;
        }
        var spellInstance = AbilityManager.CreateInstance<ISpell>(abilityDefinition.Name);
        if (spellInstance == null)
        {
            Logger.LogError("Spell '{spellName}' on item {item} cannot be instantiated.", spellName, item.DebugName);
            return StringHelpers.SomethingGoesWrong;
        }
        var spellActionInput = new SpellActionInput(abilityDefinition, Actor, spellLevel, new CastFromItemOptions { Item = item }, CommandParser.NoParameters);
        var spellInstanceGuards = spellInstance.Setup(spellActionInput);
        if (spellInstanceGuards != null)
            return spellInstanceGuards;
        spellInstance.Execute();
        return null;
    }
}
