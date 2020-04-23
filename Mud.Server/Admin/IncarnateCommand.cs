using System.Linq;
using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("incarnate", Category = "Admin", CannotBeImpersonated = true)]
        [Syntax(
            "[cmd]",
            "[cmd] room <name|id>",
            "[cmd] item name",
            "[cmd] mob name")]
        protected virtual bool DoIncarnate(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Incarnating != null)
                {
                    Wiznet.Wiznet($"{DisplayName} stops incarnating {Incarnating.DebugName}.", Domain.WiznetFlags.Incarnate);

                    Send("%M%You stop incarnating %C%{0}%x%.", Incarnating.DisplayName);
                    StopIncarnating();
                    return true;
                }
                Send("Syntax: Incarnate");
                Send("        Incarnate room name|id");
                Send("        Incarnate item name");
                Send("        Incarnate mob name");
                return true;
            }
            //
            if (parameters.Length == 1)
            {
                Send("Syntax: Incarnate");
                Send("        Incarnate room name|id");
                Send("        Incarnate item name");
                Send("        Incarnate mob name");
                return true;
            }
            //
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
            {
                Send("Target not found.");
                return true;
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
            return true;
        }
    }
}
