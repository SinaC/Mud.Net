using System.Linq;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("incarnate", Category = "Admin", CannotBeImpersonated = true)]
        protected virtual bool DoIncarnate(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Incarnating != null)
                {
                    Server.Wiznet($"{DisplayName} stops incarnating {Incarnating.DebugName}.", WiznetFlags.Incarnate);

                    Send("%M%You stop incarnating %C%{0}%x%.", Incarnating.DisplayName);
                    StopIncarnating();
                }
                else
                    Send("Syntax: Incarnate <room|item|mob> <name|id>");
            }
            else if (parameters.Length == 1)
                Send("Syntax: Incarnate <room|item|mob> <name|id>");
            else if (parameters.Length == 2)
            {
                IEntity incarnateTarget = null;
                string kind = parameters[0].Value;
                if ("room".StartsWith(kind))
                {
                    if (parameters[1].IsNumber)
                    {
                        int id = parameters[1].AsNumber;
                        incarnateTarget = World.Rooms.FirstOrDefault(x => x.Blueprint?.Id == id);
                    }
                    else
                        incarnateTarget = FindHelpers.FindByName(World.Rooms, parameters[1]);
                }
                else if ("item".StartsWith(kind))
                    incarnateTarget = FindHelpers.FindByName(World.Items, parameters[1]);
                else if ("mob".StartsWith(kind))
                    incarnateTarget = FindHelpers.FindByName(World.Characters, parameters[1]);
                if (incarnateTarget == null)
                    Send("Target not found");
                else
                {
                    //Log.Default.WriteLine(LogLevels.Info, $"{DisplayName} incarnates {incarnateTarget.DisplayName}");
                    if (Incarnating != null)
                    {
                        Server.Wiznet($"{DisplayName} stops incarnating {Incarnating.DebugName}.", WiznetFlags.Incarnate);

                        Send("%M%You stop incarnating %C%{0}%x%.", Incarnating.DisplayName);
                        Incarnating.ChangeIncarnation(null);
                    }

                    Server.Wiznet($"{DisplayName} starts incarnating {incarnateTarget.DebugName}.", WiznetFlags.Incarnate);

                    Send("%M%You start incarnating %C%{0}%x%.", incarnateTarget.DisplayName);
                    incarnateTarget.ChangeIncarnation(this);
                    Incarnating = incarnateTarget;
                    PlayerState = PlayerStates.Impersonating;
                }
            }
            return true;
        }
    }
}
