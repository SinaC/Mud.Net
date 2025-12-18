using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Alias;

// TODO: exactly the same code in Mud.Server.Player.Alias
[PlayableCharacterCommand("unalias", "Alias")]
[Alias("unmacro")]
[Syntax("[cmd] <word>")]
[Help(
@"The [cmd] command allows removing an already defined alias. (see alias)")]
public class Unalias : PlayableCharacterGameAction
{
    protected string TargetAlias { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Unalias what?";
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
