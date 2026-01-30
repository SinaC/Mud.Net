using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.GameAction;

public abstract class GameActionBase<TActor, TGameActionInfo> : IGameAction
    where TActor: class, IActor
    where TGameActionInfo: class, IGameActionInfo
{
    protected TGameActionInfo GameActionInfo { get; private set; } = default!;
    protected string Command { get; private set; } = default!;

    public TActor Actor { get; private set; } = default!;

    public abstract void Execute(IActionInput actionInput);

    public virtual string? Guards(IActionInput actionInput)
    {
        var gameActionInfo = actionInput.GameActionInfo as TGameActionInfo;
        GameActionInfo = gameActionInfo!;
        Command = actionInput.Command;
        if (GameActionInfo == null)
            return $"Internal error: GameActionInfo is null or not of type {typeof(TGameActionInfo).Name}. GameActionInfo type is {actionInput.GameActionInfo.GetType().Name}.";
        var actor = actionInput.Actor as TActor;
        Actor = actor!;
        if (Actor == null)
            return $"This command must be executed by {typeof(TActor).Name}";

        var actorGuards = (actionInput.GameActionInfo as ActorGameActionInfo)?.ActorGuards;
        if (actorGuards != null && actorGuards.Length > 0)
        {
            foreach (var guard in actorGuards)
            {
                var guardResult = guard.Guards(actionInput.Actor, actionInput, this);
                if (guardResult != null)
                    return guardResult;
            }
        }

        return null;
    }

    public string BuildCommandSyntax()
        => BuildCommandSyntax(Command, GameActionInfo.Syntax, false).ToString();

    public static StringBuilder BuildCommandSyntax(string commandNames, IEnumerable<string> syntaxes, bool addSpaces)
    {
        StringBuilder sb = new ();
        foreach (string syntax in syntaxes)
        {
            // TODO: enrich argument such as <character>, <player name>, ...
            string enrichedSyntax = syntax.Replace("[cmd]", commandNames);
            if (addSpaces)
                sb.Append("      ");
            sb.AppendLine("Syntax: " + enrichedSyntax);
        }
        return sb;
    }
}
