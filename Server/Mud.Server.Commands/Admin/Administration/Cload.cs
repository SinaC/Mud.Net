using Mud.Blueprints.Character;
using Mud.Domain;
using Mud.Flags;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("cload", "Admin")]
[Alias("mload")]
[Syntax("[cmd] <id>")]
[Help(
@"The [cmd] command is used to load new mobiles (use clone to 
duplicate strung mobs).  The vnums can be found with the vnum
command, or by stat'ing an existing mob.

Mobiles are always put into the same room as the god.")]
public class Cload : AdminGameAction
{
    private ICharacterManager CharacterManager { get; }
    private IWiznet Wiznet { get; }

    protected override IGuard<IAdmin>[] Guards => [new MustBeImpersonated(), new RequiresAtLeastOneArgument()];

    public Cload(ICharacterManager characterManager, IWiznet wiznet)
    {
        CharacterManager = characterManager;
        Wiznet = wiznet;
    }

    private int BlueprintId { get; set; }
    private CharacterBlueprintBase CharacterBlueprint { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();

        BlueprintId = actionInput.Parameters[0].AsNumber;

        CharacterBlueprint = CharacterManager.GetCharacterBlueprint(BlueprintId)!;
        if (CharacterBlueprint == null)
            return "No character with that id.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var character = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), CharacterBlueprint, Impersonating.Room);
        if (character == null)
        {
            Wiznet.Log($"DoCload: character with id {BlueprintId} cannot be created", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            Actor.Send("Character cannot be created.");
            return;
        }

        Wiznet.Log($"{Actor.DisplayName} loads {character.DebugName}.", new WiznetFlags("Load"));

        Impersonating.Act(ActOptions.ToAll, "{0:N} {0:h} created {1:n}!", Actor.Impersonating!, character);
        Actor.Send("Ok.");
    }
}
