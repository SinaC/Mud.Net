using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Logger;
using Mud.Server.Constants;
using Mud.Server.Input;
using Mud.Server.Server;

namespace Mud.Server.Player
{
    // TODO: find a better filename
    public partial class Player
    {

        [Command("macro", Category = "Misc")]
        [Command("alias", Category = "Misc")]
        protected virtual bool DoAlias(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Aliases.Any())
                {
                    Send("Your current aliases are:");
                    foreach (KeyValuePair<string, string> alias in Aliases.OrderBy(x => x.Key))
                        Send("     {0}: {1}", alias.Key, alias.Value);
                }
                else
                    Send("You have no aliases defined.");
            }
            else if (parameters.Length == 1)
            {
                string alias = parameters[0].Value.ToLowerInvariant();
                string cmd;
                if (Aliases.TryGetValue(alias, out cmd))
                    Send($"{alias} is aliases to {cmd}.");
                else
                    Send("That alias is not defined.");
            }
            else if (parameters.Length == 2)
            {
                // TODO: else add alias (!!! cannot set an alias on alias or delete :p)
                string alias = parameters[0].Value.ToLowerInvariant().Trim();
                string newCmd = parameters[1].Value;

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
                string oldCmd;
                if (Aliases.TryGetValue(alias, out oldCmd))
                {
                    Aliases[alias] = newCmd;
                    Send($"{alias} is now realiased to '{newCmd}'.");
                }
                else
                {
                    Aliases.Add(alias, newCmd);
                    Send($"{alias} is now aliased to '{newCmd}'.");
                }
            }
            else
                Send("Syntax:" + Environment.NewLine +
                     " alias" + Environment.NewLine +
                     " alias <alias>" + Environment.NewLine +
                     " alias <alias> <command>");
            return true;
        }

        [Command("unmacro", Category = "Misc")]
        [Command("unalias", Category = "Misc")]
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
                Aliases.Remove(alias);
                Send("Alias removed.");
            }
            else
                Send("No alias of that name to remove.");
            return true;
        }

        [Command("save", Category = "Misc", Priority = 999 /*low priority*/, NoShortcut = true)]
        protected virtual bool DoSave(string rawParameters, params CommandParameter[] parameters)
        {
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
            Repository.Server.Wiznet($"{DisplayName} rejoins the real world.", WiznetFlags.Logins);

            Save();
            Repository.Server.Quit(this);
            return true;
        }

        [Command("password", Category = "Misc", Priority = 999, NoShortcut = true)]
        protected virtual bool DoPassword(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length != 2)
            {
                Send("Syntax: password <old> <new>");
                return true;
            }
            if (!Repository.LoginManager.CheckPassword(Name, parameters[0].Value))
            {
                Send("Wrong password. Wait 10 seconds.");
                SetGlobalCooldown(10*ServerOptions.PulsePerSeconds);
                return true;
            }
            if (parameters[1].Value.Length < 5)
            {
                Send("New password must be at least five characters long.");
                return true;
            }
            Repository.LoginManager.ChangePassword(Name, parameters[1].Value);
            return true;
        }

        [Command("bug", Category = "Misc", Priority = 50)]
        protected virtual bool DoBug(string rawParameters, params CommandParameter[] parameters)
        {
            if (String.IsNullOrWhiteSpace(rawParameters))
            {
                Send("Report which bug?");
                return true;
            }
            string msg = $"****USER BUG REPORTING -- {DisplayName}: {rawParameters}";
            Log.Default.WriteLine(LogLevels.Warning, msg); // TODO: specific log file ?
            Repository.Server.Wiznet(msg, WiznetFlags.Bugs, AdminLevels.Implementor);
            Send("Bug logged.");
            return true;
        }

        [Command("typo", Category = "Misc", Priority = 50)]
        protected virtual bool DoTypo(string rawParameters, params CommandParameter[] parameters)
        {
            if (String.IsNullOrWhiteSpace(rawParameters))
            {
                Send("Report which typo?");
                return true;
            }
            string msg = $"****USER TYPO REPORTING -- {DisplayName}: {rawParameters}";
            Log.Default.WriteLine(LogLevels.Warning, msg); // TODO: specific log file ?
            Repository.Server.Wiznet(msg, WiznetFlags.Typos);
            Send("Typo logged.");
            return true;
        }
    }
}
