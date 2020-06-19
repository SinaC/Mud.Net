using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter.Alias
{
    // TODO: exactly the same code in Mud.Server.Player.Alias
    [PlayableCharacterCommand("macro", "Misc")]
    [PlayableCharacterCommand("alias", "Misc")]
    [Syntax(
            "[cmd]",
            "[cmd] <word>",
            "[cmd] <word> <substitution>")]
    public class Alias : PlayableCharacterGameAction
    {
        public bool DisplayAll { get; protected set; }
        public bool DisplayAlias { get; protected set; }
        public string TargetAlias { get; protected set; }
        public string Command { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
            {
                DisplayAll = true;
                return null;
            }

            TargetAlias = actionInput.Parameters[0].Value.ToLowerInvariant().Trim();
            if (actionInput.Parameters.Length == 1)
            {
                string cmd;
                if (!Actor.Aliases.TryGetValue(TargetAlias, out cmd))
                    return "That alias is not defined.";
                DisplayAlias = true;
                Command = cmd;
                return null;
            }

            if (TargetAlias.StartsWith("alias") || TargetAlias.StartsWith("unalias"))
                return "Sorry, that word is reserved.";
            if (TargetAlias.Any(c => c == '\'' || c == '"' || c == ' '))
                return "Aliases with that kind of characters are not allowed!";
            if (TargetAlias.StartsWith("delete"))
                return "That shall not be done.";

            Command = CommandHelpers.JoinParameters(actionInput.Parameters.Skip(1)); // merge parameters except first one
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (DisplayAll)
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

            if (DisplayAlias)
            {
                Actor.Send($"{TargetAlias} is aliases to {Command}.");
                return;
            }

            if (Actor.Aliases.ContainsKey(TargetAlias))
            {
                Actor.SetAlias(TargetAlias, Command);
                Actor.Send($"{TargetAlias} is now realiased to '{Command}'.");
            }
            else
            {
                Actor.SetAlias(TargetAlias, Command);
                Actor.Send($"{TargetAlias} is now aliased to '{Command}'.");
            }
        }
    }
}
