using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    // TODO: find a better filename
    public partial class Player
    {
        [Command("macro", Category = "Misc")]
        [Command("alias", Category = "Misc")]
        [Syntax(
            "[cmd]",
            "[cmd] <word>",
            "[cmd] <word> <substitution>")]
        protected virtual bool DoAlias(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Aliases.Count == 0)
                {
                    Send("You have no aliases defined.");
                    return true;
                }
                Send("Your current aliases are:");
                foreach (KeyValuePair<string, string> aliasToDisplay in Aliases.OrderBy(x => x.Key))
                    Send("     {0}: {1}", aliasToDisplay.Key, aliasToDisplay.Value);
                return true;
            }
            if (parameters.Length == 1)
            {
                string aliasToRemove = parameters[0].Value.ToLowerInvariant();
                string cmd;
                if (Aliases.TryGetValue(aliasToRemove, out cmd))
                    Send($"{aliasToRemove} is aliases to {cmd}.");
                else
                    Send("That alias is not defined.");
                return true;
            }

            string alias = parameters[0].Value.ToLowerInvariant().Trim();
            if (alias.StartsWith("alias") || alias.StartsWith("unalias"))
            {
                Send("Sorry, that word is reserved.");
                return true;
            }
            if (alias.Any(c => c == '\'' || c == '"' || c == ' '))
            {
                Send("Aliases with that kind of characters are not allowed!");
                return true;
            }
            if (alias.StartsWith("delete"))
            {
                Send("That shall not be done.");
                return true;
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
            return true;

        }

        [Command("unmacro", Category = "Misc")]
        [Command("unalias", Category = "Misc")]
        [Syntax("[cmd] <word>")]
        protected virtual bool DoUnAlias(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Unalias what?");
                return true;
            }
            string alias = parameters[0].Value.ToLowerInvariant().Trim();
            string cmd;
            if (Aliases.TryGetValue(alias, out cmd))
            {
                _aliases.Remove(alias);
                Send("Alias removed.");
            }
            else
                Send("No alias of that name to remove.");
            return true;
        }

        [Command("save", Category = "Misc", Priority = 999 /*low priority*/, NoShortcut = true)]
        protected virtual bool DoSave(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating != null)
            {
                if (Impersonating.Fighting != null)
                {
                    Send("No way! You are fighting.");
                    return true;
                }
                if (Impersonating.Position == Positions.Stunned)
                {
                    Send("You can't leave while stunned.");
                    return true;
                }
            }

            bool saved = Save();

            if (saved)
                Send("Saved");
            else
                Send("%r%Save failed%x%");

            return true;
        }

        [Command("quit", Category = "Misc", Priority = 999/*low priority*/, NoShortcut = true)]
        protected virtual bool DoQuit(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating != null)
            {
                if (Impersonating.Fighting != null)
                {
                    Send("No way! You are fighting.");
                    return true;
                }
                if (Impersonating.Position == Positions.Stunned)
                {
                    Send("You can't leave while stunned.");
                    return true;
                }
            }

            Send("Alas, all good things must come to an end.");
            Impersonating?.Act(ActOptions.ToRoom, "{0:N} has left the game.", Impersonating);
            Wiznet.Wiznet($"{DisplayName} rejoins the real world.", WiznetFlags.Logins);

            Save();
            ServerPlayerCommand.Quit(this);
            return true;
        }

        [Command("password", Category = "Misc", Priority = 999, NoShortcut = true)]
        [Syntax("[cmd] <old-password> <new-password>")]
        protected virtual bool DoPassword(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating != null)
            {
                if (Impersonating.Fighting != null)
                {
                    Send("No way! You are fighting.");
                    return true;
                }
                if (Impersonating.Position == Positions.Stunned)
                {
                    Send("You can't leave while stunned.");
                    return true;
                }
            }

            if (parameters.Length != 2)
            {
                Send("Syntax: password <old> <new>");
                return true;
            }
            if (!LoginRepository.CheckPassword(Name, parameters[0].Value))
            {
                Send("Wrong password. Wait 10 seconds.");
                SetGlobalCooldown(10* Settings.PulsePerSeconds);
                return true;
            }
            if (parameters[1].Value.Length < 5)
            {
                Send("New password must be at least five characters long.");
                return true;
            }
            LoginRepository.ChangePassword(Name, parameters[1].Value);
            return true;
        }

        [Command("delete", Category = "Misc", Priority = 999, NoShortcut = true)]
        [Syntax("[cmd] <password>")]
        protected virtual bool DoDelete(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating != null)
            {
                if (Impersonating.Fighting != null)
                {
                    Send("No way! You are fighting.");
                    return true;
                }
                if (Impersonating.Position == Positions.Stunned)
                {
                    Send("You can't leave while stunned.");
                    return true;
                }
            }

            if (parameters.Length == 0)
            {
                Send("Syntax: delete <password>");
                return true;
            }

            if (!LoginRepository.CheckPassword(Name, parameters[0].Value))
            {
                Send("Wrong password. Wait 10 seconds.");
                SetGlobalCooldown(10 * Settings.PulsePerSeconds);
                return true;
            }

            if (!DeletionConfirmationNeeded)
            {
                Send("Ask you sure you want to delete your account? Use 'delete' again to confirm.");
                DeletionConfirmationNeeded = true;
                return true;
            }

            // perform deletion
            Send("Deletion confirmed! Processing...");
            ServerPlayerCommand.Delete(this);

            return true;
        }

        [Command("bug", Category = "Misc", Priority = 50)]
        [Syntax("[cmd] <message>")]
        protected virtual bool DoBug(string rawParameters, params CommandParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(rawParameters))
            {
                Send("Report which bug?");
                return true;
            }
            string msg = $"****USER BUG REPORTING -- {DisplayName}: {rawParameters}";
            Log.Default.WriteLine(LogLevels.Warning, msg); // TODO: specific log file ?
            Wiznet.Wiznet(msg, WiznetFlags.Bugs, AdminLevels.Implementor);
            Send("Bug logged.");
            return true;
        }

        [Command("typo", Category = "Misc", Priority = 50)]
        [Syntax("[cmd] <message>")]
        protected virtual bool DoTypo(string rawParameters, params CommandParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(rawParameters))
            {
                Send("Report which typo?");
                return true;
            }
            string msg = $"****USER TYPO REPORTING -- {DisplayName}: {rawParameters}";
            Log.Default.WriteLine(LogLevels.Warning, msg); // TODO: specific log file ?
            Wiznet.Wiznet(msg, WiznetFlags.Typos);
            Send("Typo logged.");
            return true;
        }
    }
}
