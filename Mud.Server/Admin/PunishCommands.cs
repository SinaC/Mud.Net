using System.Linq;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("force", Category = "Punish")]
        [Syntax(
            "[cmd] <character> <command>",
            "[cmd] all <command>")]
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

            string command = CommandHelpers.JoinParameters(parameters.Skip(1));
            if (parameters[0].IsAll)
            {
                Send("You force everyone to '{1}'.", command);
                Wiznet.Wiznet($"{DisplayName} forces everyone to {command}", Domain.WiznetFlags.Punish);

                foreach (ICharacter victim in World.Characters.Where(x => x != this))
                {
                    victim.Send("{0} forces you to '{1}'.", DisplayName, command);
                    victim.ProcessCommand(command);
                }
            }
            else
            {
                ICharacter victim = Impersonating == null
                    ? FindHelpers.FindByName(World.Characters, parameters[0])
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

                victim.Send("{0} forces you to '{1}'.", DisplayName, command);
                Send("You force {0} to '{1}'.", victim.DisplayName, command);
                Wiznet.Wiznet($"{DisplayName} forces {victim.DebugName} to {command}", Domain.WiznetFlags.Punish);

                victim.ProcessCommand(command);
            }

            return true;
        }

        [Command("addlag", Category = "Punish")]
        [Syntax("[cmd] <player name> <tick>")]

        protected virtual bool DoAddLag(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Add lag to whom?");
                return true;
            }
            if (parameters.Length >= 1)
            {
                Send("Add how many lag ?");
                return true;
            }
            //
            int count;
            int.TryParse(parameters[1].Value, out count);
            if (count <= 0)
                Send("That makes a LOT of sense.");
            else if (count > 100)
                Send("There's a limit to cruel and unusual punishment.");
            else
            {
                IPlayer victim = PlayerManager.GetPlayer(parameters[0], true);
                if (victim == null)
                    Send(StringHelpers.CharacterNotFound);
                else
                {
                    Wiznet.Wiznet($"{DisplayName} adds lag {victim.DisplayName}.", Domain.WiznetFlags.Punish);

                    Send("Adding lag now.");
                    victim.SetGlobalCooldown(count);
                }
            }
            return true;
        }

        [Command("snoop", Category = "Punish")]
        [Syntax("[cmd] <player name>")]
        protected virtual bool DoSnoop(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Snoop whom?");
                return true;
            }

            IPlayer whom = FindHelpers.FindByName(PlayerManager.Players, parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            if (whom == this)
            {
                Send("Cancelling all snoops.");
                Wiznet.Wiznet($"{DisplayName} stops being such as snoop.", Domain.WiznetFlags.Punish | Domain.WiznetFlags.Snoops);
                foreach (IPlayer player in PlayerManager.Players)
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

            Wiznet.Wiznet($"{DisplayName} starts snooping {whom.DisplayName}.", Domain.WiznetFlags.Snoops | Domain.WiznetFlags.Punish);

            whom.SetSnoopBy(this);
            Send("Ok.");

            return true;
        }
    }
}
