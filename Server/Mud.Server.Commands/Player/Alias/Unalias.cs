using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Alias;

// TODO: avoid duplicate code, there is exactly the same code in Mud.Server.Character.PlayableCharacter.Unalias
[PlayerCommand("unalias", "Alias")]
[Alias("unmacro")]
[Syntax("[cmd] <word>")]
[Help(
@"The [cmd] command allows removing an already defined alias. (see alias)")]
public class Unalias : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [new RequiresAtLeastOneArgument { Message = "Unalias what ?"}];

    private string TargetAlias { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        TargetAlias = actionInput.Parameters[0].Value.ToLowerInvariant().Trim();
        if (!Actor.Aliases.ContainsKey(TargetAlias))
            return "No alias of that name to remove.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.RemoveAlias(TargetAlias);
        Actor.Send("Alias removed.");
    }
}
