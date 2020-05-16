using Mud.Domain;
using Mud.Server.Helpers;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [CharacterCommand("order", "Group", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <command>")]
        protected virtual CommandExecutionResults DoOrder(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Order what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            if (Slave == null)
            {
                Send("You have no followers here.");
                return CommandExecutionResults.NoExecution;
            }
            if (Slave.Room != Room)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            Slave.Send("{0} orders you to '{1}'.", DisplayName, rawParameters);
            Slave.ProcessCommand(rawParameters);
            if (this is IPlayableCharacter playableCharacter)
                playableCharacter.ImpersonatedBy?.SetGlobalCooldown(3);
            //Send("You order {0} to {1}.", Slave.Name, rawParameters);
            Send("Ok.");
            return CommandExecutionResults.Ok;
        }
    }
}
