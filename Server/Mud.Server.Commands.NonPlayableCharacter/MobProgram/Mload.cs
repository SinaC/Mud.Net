using Microsoft.Extensions.Logging;
using Mud.Blueprints.Character;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpmload", "MobProgram", Hidden = true)]
[Help("Lets the mobile load another mobile.")]
[Syntax("mob mload [vnum]")]
public class Mload : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    private ILogger<Mload> Logger { get; }
    private ICharacterManager CharacterManager { get; }

    public Mload(ILogger<Mload> logger, ICharacterManager characterManager)
    {
        Logger = logger;
        CharacterManager = characterManager;
    }

    private int BlueprintId { get; set; }
    private CharacterBlueprintBase CharacterBlueprint { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!actionInput.Parameters[0].IsNumber)
            return "load which mob ?";

        BlueprintId = actionInput.Parameters[0].AsNumber;

        CharacterBlueprint = CharacterManager.GetCharacterBlueprint(BlueprintId)!;
        if (CharacterBlueprint == null)
            return "No character with that id.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var character = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), CharacterBlueprint, Actor.Room);
        if (character == null)
        {
            Logger.LogError("MPMload: character with id {blueprintId} cannot be created", BlueprintId);
            return;
        }
    }
}
