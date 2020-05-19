using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Player
{
    // TODO: find a better filename
    public partial class Player
    {
        [Command("macro", "Misc")]
        [Command("alias", "Misc")]
        [Syntax(
            "[cmd]",
            "[cmd] <word>",
            "[cmd] <word> <substitution>")]
        protected virtual CommandExecutionResults DoAlias(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Aliases.Count == 0)
                {
                    Send("You have no aliases defined.");
                    return CommandExecutionResults.Ok;
                }
                Send("Your current aliases are:");
                foreach (KeyValuePair<string, string> aliasToDisplay in Aliases.OrderBy(x => x.Key))
                    Send("     {0}: {1}", aliasToDisplay.Key, aliasToDisplay.Value);
                return CommandExecutionResults.Ok;
            }
            if (parameters.Length == 1)
            {
                string aliasToRemove = parameters[0].Value.ToLowerInvariant();
                string cmd;
                if (Aliases.TryGetValue(aliasToRemove, out cmd))
                {
                    Send($"{aliasToRemove} is aliases to {cmd}.");
                    return CommandExecutionResults.Ok;
                }
                else
                {
                    Send("That alias is not defined.");
                    return CommandExecutionResults.InvalidParameter;
                }
            }

            string alias = parameters[0].Value.ToLowerInvariant().Trim();
            if (alias.StartsWith("alias") || alias.StartsWith("unalias"))
            {
                Send("Sorry, that word is reserved.");
                return CommandExecutionResults.InvalidParameter;
            }
            if (alias.Any(c => c == '\'' || c == '"' || c == ' '))
            {
                Send("Aliases with that kind of characters are not allowed!");
                return CommandExecutionResults.InvalidParameter;
            }
            if (alias.StartsWith("delete"))
            {
                Send("That shall not be done.");
                return CommandExecutionResults.InvalidParameter;
            }
            string newCmd = CommandHelpers.JoinParameters(parameters.Skip(1)); // merge parameters except first one
            if (_aliases.TryGetValue(alias, out _))
            {
                _aliases[alias] = newCmd;
                Send($"{alias} is now realiased to '{newCmd}'.");
            }
            else
            {
                _aliases.Add(alias, newCmd);
                Send($"{alias} is now aliased to '{newCmd}'.");
            }
            return CommandExecutionResults.Ok;

        }

        [Command("unmacro", "Misc")]
        [Command("unalias", "Misc")]
        [Syntax("[cmd] <word>")]
        protected virtual CommandExecutionResults DoUnAlias(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Unalias what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            string alias = parameters[0].Value.ToLowerInvariant().Trim();
            if (Aliases.TryGetValue(alias, out _))
            {
                _aliases.Remove(alias);
                Send("Alias removed.");
                return CommandExecutionResults.Ok;
            }
            Send("No alias of that name to remove.");
            return CommandExecutionResults.InvalidParameter;
        }

        [Command("save", "Misc", Priority = 999 /*low priority*/, NoShortcut = true)]
        protected virtual CommandExecutionResults DoSave(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating != null)
            {
                if (Impersonating.Fighting != null)
                {
                    Send("No way! You are fighting.");
                    return CommandExecutionResults.NoExecution;
                }
                if (Impersonating.Position == Positions.Stunned)
                {
                    Send("You can't leave while stunned.");
                    return CommandExecutionResults.NoExecution;
                }
            }

            bool saved = Save();
            if (saved)
            {
                Send("Saving. Remember that ROM has automatic saving now.");
                return CommandExecutionResults.Ok;
            }
            
            Send("%r%Save failed%x%");
            return CommandExecutionResults.Error;
        }

        [Command("quit", "Misc", Priority = 999/*low priority*/, NoShortcut = true)]
        protected virtual CommandExecutionResults DoQuit(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO
            // cannot quit while affected by an harmful aura/ periodic aura
            // while leaving(or reconnecting) aura should be removed ? maybe an additional flag saying if timer should decrease while disconnected ?
            if (Impersonating != null)
            {
                if (Impersonating.Fighting != null)
                {
                    Send("No way! You are fighting.");
                    return CommandExecutionResults.NoExecution;
                }
                if (Impersonating.Position == Positions.Stunned)
                {
                    Send("You can't leave while stunned.");
                    return CommandExecutionResults.NoExecution;
                }
            }

            Send("Alas, all good things must come to an end.");
            Impersonating?.Act(ActOptions.ToRoom, "{0:N} has left the game.", Impersonating);
            Wiznet.Wiznet($"{DisplayName} rejoins the real world.", WiznetFlags.Logins);

            Save();
            ServerPlayerCommand.Quit(this);
            return CommandExecutionResults.Ok;
        }

        [Command("password", "Misc", Priority = 999, NoShortcut = true)]
        [Syntax("[cmd] <old-password> <new-password>")]
        protected virtual CommandExecutionResults DoPassword(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating != null)
            {
                if (Impersonating.Fighting != null)
                {
                    Send("No way! You are fighting.");
                    return CommandExecutionResults.NoExecution;
                }
                if (Impersonating.Position == Positions.Stunned)
                {
                    Send("You can't leave while stunned.");
                    return CommandExecutionResults.NoExecution;
                }
            }

            if (parameters.Length != 2)
                return CommandExecutionResults.SyntaxError;
            if (!LoginRepository.CheckPassword(Name, parameters[0].Value))
            {
                Send("Wrong password. Wait 10 seconds.");
                SetGlobalCooldown(10* Pulse.PulsePerSeconds);
                return CommandExecutionResults.InvalidParameter;
            }
            if (parameters[1].Value.Length < 5)
            {
                Send("New password must be at least five characters long.");
                return CommandExecutionResults.InvalidParameter;
            }
            LoginRepository.ChangePassword(Name, parameters[1].Value);
            return CommandExecutionResults.Ok;
        }

        [Command("delete", "Misc", Priority = 999, NoShortcut = true)]
        [Syntax("[cmd] <password>")]
        protected virtual CommandExecutionResults DoDelete(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating != null)
            {
                if (Impersonating.Fighting != null)
                {
                    Send("No way! You are fighting.");
                    return CommandExecutionResults.NoExecution;
                }
                if (Impersonating.Position == Positions.Stunned)
                {
                    Send("You can't leave while stunned.");
                    return CommandExecutionResults.NoExecution;
                }
            }

            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            if (!LoginRepository.CheckPassword(Name, parameters[0].Value))
            {
                Send("Wrong password. Wait 10 seconds.");
                SetGlobalCooldown(10 * Pulse.PulsePerSeconds);
                return CommandExecutionResults.InvalidParameter;
            }

            if (!DeletionConfirmationNeeded)
            {
                Send("Ask you sure you want to delete your account? Use 'delete' again to confirm.");
                DeletionConfirmationNeeded = true;
                return CommandExecutionResults.Ok;
            }

            // perform deletion
            Send("Deletion confirmed! Processing...");
            ServerPlayerCommand.Delete(this);

            return CommandExecutionResults.Ok;
        }

        [Command("bug", "Misc", Priority = 50)]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoBug(string rawParameters, params CommandParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(rawParameters))
            {
                Send("Report which bug?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            Wiznet.Wiznet($"****USER BUG REPORTING -- {DisplayName}: {rawParameters}", WiznetFlags.Bugs, AdminLevels.Implementor);
            Send("Bug logged.");
            return CommandExecutionResults.Ok;
        }

        [Command("typo", "Misc", Priority = 50)]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoTypo(string rawParameters, params CommandParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(rawParameters))
            {
                Send("Report which typo?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            Wiznet.Wiznet($"****USER TYPO REPORTING -- {DisplayName}: {rawParameters}", WiznetFlags.Typos);
            Send("Typo logged.");
            return CommandExecutionResults.Ok;
        }
    }
}
