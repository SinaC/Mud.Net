using System.Linq;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Player;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("force", "Punish")]
        [Syntax(
            "[cmd] <character> <command>",
            "[cmd] all <command>")]
        protected virtual CommandExecutionResults DoForce(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
                return CommandExecutionResults.SyntaxError;

            if (parameters[1].Value == "delete")
            {
                Send("That will NOT be done.");
                return CommandExecutionResults.InvalidParameter;
            }

            string command = CommandHelpers.JoinParameters(parameters.Skip(1));
            if (parameters[0].IsAll)
            {
                Send("You force everyone to '{1}'.", command);
                Wiznet.Wiznet($"{DisplayName} forces everyone to {command}", Domain.WiznetFlags.Punish);

                foreach (ICharacter victimLoop in World.Characters.Where(x => x != Impersonating))
                {
                    victimLoop.Send("{0} forces you to '{1}'.", DisplayName, command);
                    victimLoop.ProcessCommand(command);
                }
                return CommandExecutionResults.Ok;
            }
            ICharacter victim = Impersonating == null
                ? FindHelpers.FindByName(World.Characters, parameters[0])
                : FindHelpers.FindChararacterInWorld(Impersonating, parameters[0]);
            if (victim == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            if (victim == Impersonating || victim == Incarnating)
            {
                Send("Aye aye, right away!");
                return CommandExecutionResults.InvalidTarget;
            }

            victim.Send("{0} forces you to '{1}'.", DisplayName, command);
            Send("You force {0} to '{1}'.", victim.DisplayName, command);
            Wiznet.Wiznet($"{DisplayName} forces {victim.DebugName} to {command}", Domain.WiznetFlags.Punish);

            victim.ProcessCommand(command);

            return CommandExecutionResults.Ok;
        }

        [AdminCommand("addlag", "Punish")]
        [Syntax("[cmd] <player name> <tick>")]

        protected virtual CommandExecutionResults DoAddLag(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
                return CommandExecutionResults.SyntaxError;

            //
            int count;
            int.TryParse(parameters[1].Value, out count);
            if (count <= 0)
            {
                Send("That makes a LOT of sense.");
                return CommandExecutionResults.InvalidParameter;
            }
            if (count > 100)
            {
                Send("There's a limit to cruel and unusual punishment.");
                return CommandExecutionResults.InvalidParameter;
            }

            IPlayer victim = PlayerManager.GetPlayer(parameters[0], true);
            if (victim == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            Wiznet.Wiznet($"{DisplayName} adds lag {victim.DisplayName}.", Domain.WiznetFlags.Punish);

            Send("Adding lag now.");
            victim.SetGlobalCooldown(count);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("snoop", "Punish")]
        [Syntax("[cmd] <player name>")]
        protected virtual CommandExecutionResults DoSnoop(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            IPlayer whom = FindHelpers.FindByName(PlayerManager.Players, parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
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
                return CommandExecutionResults.Ok;
            }
            if (whom.SnoopBy != null)
            {
                Send("Busy already.");
                return CommandExecutionResults.InvalidTarget;
            }
            // Check snoop loop
            for(IAdmin snooper = SnoopBy; snooper != null; snooper = snooper.SnoopBy)
                if (snooper == whom)
                {
                    Send("No snoop loops.");
                    return CommandExecutionResults.InvalidParameter;
                }

            Wiznet.Wiznet($"{DisplayName} starts snooping {whom.DisplayName}.", Domain.WiznetFlags.Snoops | Domain.WiznetFlags.Punish);

            whom.SetSnoopBy(this);
            Send("Ok.");

            return CommandExecutionResults.Ok;
        }
    }
}
