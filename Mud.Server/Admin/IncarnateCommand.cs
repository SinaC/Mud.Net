using System.Linq;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Player;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("incarnate", "Admin", CannotBeImpersonated = true)]
        [Syntax(
            "[cmd]",
            "[cmd] room <name|id>",
            "[cmd] item name",
            "[cmd] mob name")]
        protected virtual CommandExecutionResults DoIncarnate(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Incarnating != null)
                {
                    Wiznet.Wiznet($"{DisplayName} stops incarnating {Incarnating.DebugName}.", Domain.WiznetFlags.Incarnate);

                    Send("%M%You stop incarnating %C%{0}%x%.", Incarnating.DisplayName);
                    StopIncarnating();
                    return CommandExecutionResults.Ok;
                }
                return CommandExecutionResults.SyntaxError;
            }
            //
            if (parameters.Length == 1)
                return CommandExecutionResults.SyntaxError;
            //
            IEntity incarnateTarget = null;
            string kind = parameters[0].Value;
            if ("room".StartsWith(kind))
            {
                if (parameters[1].IsNumber)
                {
                    int id = parameters[1].AsNumber;
                    incarnateTarget = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint?.Id == id);
                }
                else
                    incarnateTarget = FindHelpers.FindByName(RoomManager.Rooms, parameters[1]);
            }
            else if ("item".StartsWith(kind))
                incarnateTarget = FindHelpers.FindByName(ItemManager.Items, parameters[1]);
            else if ("mob".StartsWith(kind))
                incarnateTarget = FindHelpers.FindByName(World.Characters, parameters[1]);
            if (incarnateTarget == null)
            {
                Send("Target not found.");
                return CommandExecutionResults.TargetNotFound;
            }
            //
            if (Incarnating != null)
            {
                string msgStop = $"{DisplayName} stops incarnating {Incarnating.DebugName}.";
                Log.Default.WriteLine(LogLevels.Info, msgStop);
                Wiznet.Wiznet(msgStop, Domain.WiznetFlags.Incarnate);

                Send("%M%You stop incarnating %C%{0}%x%.", Incarnating.DisplayName);
                Incarnating.ChangeIncarnation(null);
            }

            string msStartsg = $"{DisplayName} starts incarnating {incarnateTarget.DebugName}.";
            Log.Default.WriteLine(LogLevels.Info, msStartsg);
            Wiznet.Wiznet(msStartsg, Domain.WiznetFlags.Incarnate);

            Send("%M%You start incarnating %C%{0}%x%.", incarnateTarget.DisplayName);
            incarnateTarget.ChangeIncarnation(this);
            Incarnating = incarnateTarget;
            PlayerState = PlayerStates.Impersonating;
            return CommandExecutionResults.Ok;
        }
    }
}
