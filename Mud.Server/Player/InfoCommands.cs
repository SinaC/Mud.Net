using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("scroll", "Information")]
        [Command("page", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <number>")]
        protected virtual CommandExecutionResults DoPage(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (PagingLineCount == 0)
                    Send("You do not page long messages.");
                else
                    Send($"You currently display {PagingLineCount} lines per page.");
                return CommandExecutionResults.Ok;
            }

            if (!parameters[0].IsNumber)
            {
                Send("You must provide a number.");
                return CommandExecutionResults.InvalidParameter;
            }

            int lineCount = parameters[0].AsNumber;
            if (lineCount == 0)
            {
                Send("Paging disabled");
                PagingLineCount = 0;
                return CommandExecutionResults.Ok;
            }

            if (lineCount < 10 || lineCount > 100)
            {
                Send("Please provide a reasonable number.");
                return CommandExecutionResults.InvalidParameter;
            }

            Send($"Scroll set to {lineCount} lines.");
            PagingLineCount = lineCount;

            return CommandExecutionResults.Ok;
        }       
    }
}
