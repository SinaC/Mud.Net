using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Alias;

// TODO: avoid duplicate code, there is exactly the same code in Mud.Server.Character.PlayableCharacter.Alias
[PlayerCommand("alias", "Alias")]
[Alias("macro")]
[Syntax(
        "[cmd]",
        "[cmd] <word>",
        "[cmd] <word> <substitution>")]
[Help(
@"The [cmd] command allows limited shortening of command names.  At this time,
aliases cannot call other aliases, and cannot generate more than one command.
Alias by itself lists your current aliases, Alias <word> lists the alias with 
that name (if such exist), and alias with both a word and and argument 
produces a new alias.  You cannot alias either alias or unalias to a new
command.  Examples of use:

alias gc get all corpse --> typing gc will equal typing 'get all corpse'
alias ff cast 'fireball' --> ff orc will equal typing 'cast 'fireball' orc'
alias kick use kick --> kick orc will equal typing 'use kick orc'

Only the first word on the line will be subsituted at this time.")]
public class Alias : PlayerGameAction
{
    private ICommandParser CommandParser { get; }

    public Alias(ICommandParser commandParser)
    {
        CommandParser = commandParser;
    }

    protected enum Actions
    {
        DisplayAll,
        Display,
        Assign
    }

    protected Actions Action { get; set; }
    protected string TargetAlias { get; set; } = default!;
    protected string TargetCommand { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
        {
            Action = Actions.DisplayAll;
            return null;
        }

        TargetAlias = actionInput.Parameters[0].Value.ToLowerInvariant().Trim();
        if (actionInput.Parameters.Length == 1)
        {
            if (!Actor.Aliases.TryGetValue(TargetAlias, out var cmd))
                return "That alias is not defined.";
            Action = Actions.Display;
            TargetCommand = cmd;
            return null;
        }

        if (TargetAlias.StartsWith("alias") || TargetAlias.StartsWith("unalias"))
            return "Sorry, that word is reserved.";
        if (TargetAlias.Any(c => c == '\'' || c == '"' || c == ' '))
            return "Aliases with that kind of characters are not allowed!";
        if (TargetAlias.StartsWith("delete") || TargetAlias.StartsWith("deleteavatar"))
            return "That shall not be done.";

        Action = Actions.Assign;
        TargetCommand = CommandParser.JoinParameters(actionInput.Parameters.Skip(1)); // merge parameters except first one
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Action)
        {
            case Actions.DisplayAll:
                {
                    if (Actor.Aliases.Count == 0)
                        Actor.Send("You have no aliases defined.");
                    else
                    {
                        Actor.Send("Your current aliases are:");
                        foreach (KeyValuePair<string, string> aliasToDisplay in Actor.Aliases.OrderBy(x => x.Key))
                            Actor.Send("     {0}: {1}", aliasToDisplay.Key, aliasToDisplay.Value);
                    }
                    return;
                }

            case Actions.Display:
                {
                    Actor.Send($"{TargetAlias} is aliases to {TargetCommand}.");
                    return;
                }
            case Actions.Assign:
                if (Actor.Aliases.ContainsKey(TargetAlias))
                {
                    Actor.SetAlias(TargetAlias, TargetCommand);
                    Actor.Send($"{TargetAlias} is now realiased to '{TargetCommand}'.");
                }
                else
                {
                    Actor.SetAlias(TargetAlias, TargetCommand);
                    Actor.Send($"{TargetAlias} is now aliased to '{TargetCommand}'.");
                }
                return;
        }
    }
}
