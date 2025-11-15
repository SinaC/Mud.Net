using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Alias;

// TODO: avoid duplicate code, there is exactly the same code in Mud.Server.Character.PlayableCharacter.Unalias
[PlayerCommand("unalias", "Misc")]
[Alias("unmacro")]
[Syntax("[cmd] <word>")]
public class Unalias : PlayerGameAction
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
