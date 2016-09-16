using System.Linq;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Server;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("shutdown", Category = "Admin", Priority = 999 /*low priority*/, NoShortcut = true)]
        protected virtual bool DoShutdown(string rawParameters, params CommandParameter[] parameters)
        {
            int seconds;
            if (parameters.Length == 0 || !int.TryParse(parameters[0].Value, out seconds))
                Send("Syntax: shutdown xxx  where xxx is a delay in seconds.");
            else if (seconds < 30)
                Send("You cannot shutdown that fast.");
            else
                Repository.Server.Shutdown(seconds);
            return true;
        }

        [Command("slay", Category = "Admin", NoShortcut = true)]
        protected virtual bool DoSlay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Slay whom?");
                return true;
            }
            if (Impersonating == null)
            {
                Send("Slay can only be used when impersonating.");
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
                Send("Suicide is a mortal sin.");
                return true;
            }
            //Impersonating.Act(ActOptions.ToCharacter, "You slay {0:m} in cold blood!", victim);
            //victim.Act(ActOptions.ToCharacter, "{0} slays you in cold blood!", Impersonating);
            //Impersonating.ActToNotVictim(victim, "{0} slays {1} in cold blood!", Impersonating, victim);
            Impersonating.Act(ActOptions.ToAll, "{0:N} slay{1:v} {2} in cold blood!", Impersonating, Impersonating, victim);

            Impersonating.RawKill(victim, false);

            return true;
        }

        [Command("purge", Category = "Admin", NoShortcut = true)]
        protected virtual bool DoPurge(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Purge what?");
                return true;
            }
            if (Impersonating == null)
            {
                Send("Purge can only be used when impersonating.");
                return true;
            }
            IItem item = FindHelpers.FindItemHere(Impersonating, parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemNotFound);
                return true;
            }
            Impersonating.Act(ActOptions.ToAll, "{0:N} purge{0:v} {1}!", Impersonating, item);

            Repository.World.RemoveItem(item);

            return true;
        }

        [Command("goto", Category = "Admin")]
        protected virtual bool DoGoto(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Goto where?");
                return true;
            }
            if (Impersonating == null)
            {
                Send("Goto can only be used when impersonating.");
                return true;
            }

            //
            IRoom where = FindHelpers.FindLocation(Impersonating, parameters[0]);
            if (where == null)
            {
                Send("No such location.");
                return true;
            }

            if (Impersonating.Fighting != null)
                Impersonating.StopFighting(true);
            Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0} leaves in a swirling mist.", Impersonating); // Don't display 'Someone leaves ...' if Impersonating is not visible
            Impersonating.ChangeRoom(where);
            Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0} appears in a swirling mist.", Impersonating);
            Impersonating.ProcessCommand("look");

            return true;
        }

        [Command("xpbonus", Category = "Admin")]
        protected virtual bool DpXpBonus(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Syntax: xpgain <character> <experience>");
                return true;
            }

            ICharacter victim = FindHelpers.FindByName(Repository.Server.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), parameters[0]);
            if (victim == null)
            {
                Send("That impersonated player is not here.");
                return true;
            }

            int experience = parameters[1].AsInt;
            if (experience < 1)
            {
                Send("Experience must be greather than 1.");
                return true;
            }

            if (victim.Level >= ServerOptions.MaxLevel)
            {
                Send($"{DisplayName} is already at max level.");
                return true;
            }

            victim.Send("You have received an experience boost.");
            victim.GainExperience(experience);

            //
            victim.ImpersonatedBy.Save();
            return true;
        }

        [Command("transfer", Category = "Admin")]
        protected virtual bool DoTransfer(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Transfer whom (and where)?");
                return true;
            }
            if (Impersonating == null && parameters.Length == 1)
            {
                Send("Transfer without specifying location can only be used when impersonating.");
                return true;
            }

            // TODO: IsAll ?

            IRoom where;
            if (Impersonating != null)
            {
                where = parameters.Length == 1
                    ? Impersonating.Room
                    : FindHelpers.FindLocation(Impersonating, parameters[1]);
            }
            else
                where = FindHelpers.FindLocation(parameters[1]);
            if (where == null)
            {
                Send("No such location.");
                return true;
            }

            ICharacter whom;
            if (Impersonating != null)
                whom = FindHelpers.FindChararacterInWorld(Impersonating, parameters[0]);
            else
                whom = FindHelpers.FindByName(Repository.World.Characters, parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }

            if (whom.Fighting != null)
                whom.StopFighting(true);
            whom.Act(ActOptions.ToRoom, "{0:N} disappears in a mushroom cloud.", whom);
            whom.ChangeRoom(where);
            whom.Act(ActOptions.ToRoom, "{0:N} appears from a puff of smoke.", whom);
            if (whom != Impersonating)
            {
                if (Impersonating != null)
                    whom.Act(ActOptions.ToCharacter, "{0:N} has transferred you.", Impersonating);
                else
                    whom.Act(ActOptions.ToCharacter, "Someone has transferred you.");
            }
            whom.ProcessCommand("look"); // TODO: call immediate command

            Send("Ok");
            return true;
        }
    }
}
