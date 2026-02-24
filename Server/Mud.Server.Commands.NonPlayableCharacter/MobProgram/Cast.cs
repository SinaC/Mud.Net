using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Spell;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpcast", "MobProgram", Hidden = true)]
[Syntax("mob cast [spell] {target}")]
[Help(
@"Lets the mobile cast spells --
Beware: this does only crude checking on the target validity
and does not account for mana etc., so you should do all the
necessary checking in your mob program before issuing this cmd!")]
public class Cast : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Sitting), new RequiresAtLeastOneArgument()];

    private ILogger<Cast> Logger { get; }
    private IAbilityManager AbilityManager { get; }

    public Cast(ILogger<Cast> logger, IAbilityManager abilityManager)
    {
        Logger = logger;
        AbilityManager = abilityManager;
    }

    private ISpell SpellInstance { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var spellName = actionInput.Parameters[0].Value;
        var abilityDefinition = AbilityManager.Get(spellName, AbilityTypes.Spell);
        if (abilityDefinition == null)
        {
            Logger.LogError("Spell {abilityName} doesn't exist.", spellName);
            return "You don't know any spells of that name.";
        }

        SpellInstance = AbilityManager.CreateInstance<ISpell>(abilityDefinition.Name)!;
        if (SpellInstance == null)
        {
            Logger.LogError("Spell {abilityName} cannot be created.", abilityDefinition.Name);
            return StringHelpers.SomethingGoesWrong;
        }

        var newParameters = actionInput.Parameters.Skip(1).ToArray();
        var spellActionInput = new SpellActionInput(abilityDefinition, Actor, Actor.Level, false, newParameters);
        var spellInstanceGuards = SpellInstance.Setup(spellActionInput);
        return spellInstanceGuards;
    }

    public override void Execute(IActionInput actionInput)
    {
        SpellInstance.Execute();
    }
}
