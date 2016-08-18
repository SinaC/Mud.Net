using System;
using System.Linq;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        //[Command("shutdow", Hidden = true)] // TODO: add an option in CommandAttribute to force full command to be type
        //protected virtual bool DoShutdow(string rawParameters, params CommandParameter[] parameters)
        //{
        //    Send("If you want to SHUTDOWN, spell it out." + Environment.NewLine);
        //    return true;
        //}

        [Command("shutdown", Category = "Admin", Priority = 999/*low priority*/, NoShortcut = true)]
        protected virtual bool DoShutdown(string rawParameters, params CommandParameter[] parameters)
        {
            int seconds;
            if (parameters.Length == 0 || !int.TryParse(parameters[0].Value, out seconds))
                Send("Syntax: shutdown xxx  where xxx is a delay in seconds." + Environment.NewLine);
            else if (seconds < 30)
                Send("You cannot shutdown that fast." + Environment.NewLine);
            else
                Repository.Server.Shutdown(seconds);
            return true;
        }

        [Command("slay", Category = "Admin")]
        protected virtual bool DoSlay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Slay whom?" + Environment.NewLine);
                return true;
            }
            if (Impersonating == null)
            {
                Send("Slay can only be used when impersonating." + Environment.NewLine);
                return true;
            }
            ICharacter victim = FindHelpers.FindByName(Impersonating.Room.People, parameters[0]);
            if (victim == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            if (victim == Impersonating)
            {
                Send("Suicide is a mortal sin." + Environment.NewLine);
                return true;
            }
            Impersonating.Act(ActOptions.ToCharacter, "You slay {0:m} in cold blood!", victim);
            victim.Act(ActOptions.ToCharacter, "{0} slays you in cold blood!", Impersonating);
            Impersonating.ActToNotVictim(victim, "{0} slays {1} in cold blood!", Impersonating, victim);

            Impersonating.RawKill(victim, false);

            return true;
        }

        [Command("purge", Category = "Admin")]
        protected virtual bool DoPurge(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Purge what?" + Environment.NewLine);
                return true;
            }
            if (Impersonating == null)
            {
                Send("Purge can only be used when impersonating." + Environment.NewLine);
                return true;
            }
            IItem item = FindHelpers.FindCharacterItemByName2(Impersonating, parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemNotFound);
                return true;
            }
            Impersonating.Act(ActOptions.ToCharacter, "You purge {0}!", item);
            Impersonating.Act(ActOptions.ToRoom, "{0} purges {1}", Impersonating, item);

            Repository.World.RemoveItem(item);

            return true;
        }

        [Command("force", Category = "Admin")]
        protected virtual bool DoForce(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
                Send("Force whom what?" + Environment.NewLine);
            else
            {
                ICharacter victim = FindHelpers.FindByName(Repository.World.Characters, parameters[0]);
                if (victim == null)
                    Send("Target not found.");
                else
                {
                    string command = CommandHelpers.JoinParameters(parameters.Skip(1));
                    victim.Send("{0} forces you to '{1}'." + Environment.NewLine, Name, command);
                    victim.ProcessCommand(command);
                    Send("Ok." + Environment.NewLine);
                }
            }
            return true;
        }

        [Command("addlag", Category = "Admin")]
        protected virtual bool DoAddLag(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Add lag to whom?" + Environment.NewLine);
            else if (parameters.Length == 1)
                Send("Add how many lag ?" + Environment.NewLine);
            else
            {
                int count;
                int.TryParse(parameters[1].Value, out count);
                if (count <= 0)
                    Send("That makes a LOT of sense." + Environment.NewLine);
                else if (count > 100)
                    Send("There's a limit to cruel and unusual punishment." + Environment.NewLine);
                else
                {
                    IPlayer victim = Repository.Server.GetPlayer(parameters[0], true);
                    if (victim == null)
                        Send(StringHelpers.CharacterNotFound);
                    else
                    {
                        Send("Adding lag now" + Environment.NewLine);
                        victim.SetGlobalCooldown(count);
                    }
                }
            }
            return true;
        }
    }
}
