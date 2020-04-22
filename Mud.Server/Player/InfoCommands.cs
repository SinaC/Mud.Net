using System.Text;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("who", Category = "Information")]
        protected virtual bool DoWho(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: title, additional informations
            StringBuilder sb = new StringBuilder();
            //
            sb.AppendFormatLine("Players:");
            foreach (IPlayer player in PlayerManager.Players)
            {
                IAdmin admin = player as IAdmin;
                string adminLevel = admin == null ? "" : $"[{admin.Level}]";
                switch (player.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (player.Impersonating != null)
                            sb.AppendFormatLine("[ IG]{0} {1} playing {2} [lvl: {3} Class: {4} Race: {5}]",
                                adminLevel,
                                player.DisplayName,
                                player.Impersonating.DisplayName,
                                player.Impersonating.Level,
                                player.Impersonating.Class?.DisplayName ?? "(none)",
                                player.Impersonating.Race?.DisplayName ?? "(none)");
                        else
                            sb.AppendFormatLine("[ IG] {0} {1} playing something", adminLevel, player.DisplayName);
                        break;
                    default:
                        sb.AppendFormatLine("[OOG] {0} {1}", adminLevel, player.DisplayName);
                        break;
                }
            }
            //
            Page(sb);
            return true;
        }

        [Command("areas", Category = "Information", Priority = 10)]
        protected virtual bool DoAreas(string rawParameters, params CommandParameter[] parameters)
        {
            
            StringBuilder sb = TableGenerators.AreaTableGenerator.Value.Generate("Areas", World.Areas);
            Page(sb);
            return true;
        }

        [Command("scroll", Category = "Information")]
        [Command("page", Category = "Information")]
        protected virtual bool DoPage(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (PagingLineCount == 0)
                    Send("You do not page long messages.");
                else
                    Send($"You currently display {PagingLineCount} lines per page.");
                return true;
            }

            if (!parameters[0].IsNumber)
            {
                Send("You must provide a number.");
                return true;
            }

            int lineCount = parameters[0].AsNumber;
            if (lineCount == 0)
            {
                Send("Paging disabled");
                PagingLineCount = 0;
                return true;
            }

            if (lineCount < 10 || lineCount > 100)
            {
                Send("Please provide a reasonable number.");
                return true;
            }

            Send($"Scroll set to {lineCount} lines.");
            PagingLineCount = lineCount;

            return true;
        }       
    }
}
