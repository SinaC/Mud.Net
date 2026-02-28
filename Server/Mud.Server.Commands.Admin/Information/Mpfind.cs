using Mud.Common;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.MobProgram.Interfaces;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("mpfind", "Information")]
[Syntax(
    "[cmd] <id>")]
public class Mpfind : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument()];

    private ICharacterManager CharacterManager { get; }
    private IMobProgramManager MobProgramManager { get; }

    public Mpfind(ICharacterManager characterManager, IMobProgramManager mobProgramManager)
    {
        CharacterManager = characterManager;
        MobProgramManager = mobProgramManager;
    }

    private IMobProgram MobProgram { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();

        var mobProgramId = actionInput.Parameters[0].AsNumber;
        var mobProgram = MobProgramManager.GetMobProgram(mobProgramId);

        if (mobProgram == null)
            return "Not found.";
        MobProgram = mobProgram;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var npcBlueprints = CharacterManager.CharacterBlueprints.Where(x => x.MobProgramTriggers.Any(x => x.MobProgramId == MobProgram.Blueprint.Id));

        var sb = new StringBuilder();
        foreach (var npcBlueprint in npcBlueprints)
        {
            sb.AppendFormatLine("{0} [id: {1}]", npcBlueprint.ShortDescription, npcBlueprint.Id);
            foreach (var mobProgramTrigger in npcBlueprint.MobProgramTriggers.Where(x => x.MobProgramId == MobProgram.Blueprint.Id))
                sb.AppendFormatLine("     {0}", mobProgramTrigger.ToString() ?? string.Empty);
        }
        Actor.Page(sb);
    }
}
