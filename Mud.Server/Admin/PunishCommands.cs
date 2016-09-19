using System.Linq;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("force", Category = "Punish")]
        protected virtual bool DoForce(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Force whom to do what?");
                return true;
            }
            if (parameters[1].Value == "delete")
            {
                Send("That will NOT be done.");
                return true;
            }

            ICharacter victim = Impersonating == null
                ? FindHelpers.FindByName(Repository.World.Characters, parameters[0])
                : FindHelpers.FindChararacterInWorld(Impersonating, parameters[0]);
            if (victim == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            if (victim == Impersonating || victim == Incarnating)
            {
                Send("Aye aye, right away!");
                return true;
            }

            string command = CommandHelpers.JoinParameters(parameters.Skip(1));
            victim.Send("{0} forces you to '{1}'.", Name, command);
            victim.ProcessCommand(command);
            Send("Ok.");

            Repository.Server.Wiznet($"{DisplayName} forces {victim.DebugName} to {command}", WiznetFlags.Punish);

            return true;
        }

        [Command("addlag", Category = "Punish")]
        protected virtual bool DoAddLag(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Add lag to whom?");
            else if (parameters.Length == 1)
                Send("Add how many lag ?");
            else
            {
                int count;
                int.TryParse(parameters[1].Value, out count);
                if (count <= 0)
                    Send("That makes a LOT of sense.");
                else if (count > 100)
                    Send("There's a limit to cruel and unusual punishment.");
                else
                {
                    IPlayer victim = Repository.Server.GetPlayer(parameters[0], true);
                    if (victim == null)
                        Send(StringHelpers.CharacterNotFound);
                    else
                    {
                        Repository.Server.Wiznet($"{DisplayName} adds lag {victim.DisplayName}.", WiznetFlags.Punish);

                        Send("Adding lag now.");
                        victim.SetGlobalCooldown(count);
                    }
                }
            }
            return true;
        }

        [Command("snoop", Category = "Punish")]
        protected virtual bool DoSnoop(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Snoop whom?");
                return true;
            }

            IPlayer whom = FindHelpers.FindByName(Repository.Server.Players, parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            if (whom == this)
            {
                Send("Cancelling all snoops.");
                Repository.Server.Wiznet($"{DisplayName} stops being such as snoop.", WiznetFlags.Punish | WiznetFlags.Snoops);
                foreach (IPlayer player in Repository.Server.Players)
                {
                    if (player.SnoopBy == this)
                        player.SetSnoopBy(null);
                }
                return true;
            }
            if (whom.SnoopBy != null)
            {
                Send("Busy already.");
                return true;
            }
            // Check snoop loop
            for(IAdmin snooper = SnoopBy; snooper != null; snooper = snooper.SnoopBy)
                if (snooper == whom)
                {
                    Send("No snoop loops.");
                    return true;
                }

            Repository.Server.Wiznet($"{DisplayName} starts snooping {whom.DisplayName}.", WiznetFlags.Snoops | WiznetFlags.Punish);

            whom.SetSnoopBy(this);
            Send("Ok.");

            return true;
        }
    }
}
