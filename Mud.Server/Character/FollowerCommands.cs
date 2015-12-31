using System;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("charm")]
        protected virtual bool DoCharm(string rawParameters, params CommandParameter[] parameters)
        {
            if (ControlledBy != null)
                Send("You feel like taking, not giving, orders." + Environment.NewLine);
            else if (parameters.Length == 0)
            {
                if (Slave != null)
                {
                    Send("You stop controlling {0}." + Environment.NewLine, Slave.Name);
                    Slave.ChangeController(null);
                    Slave = null;
                }
                else
                    Send("Try controlling something before trying to un-control." + Environment.NewLine);
            }
            else
            {
                ICharacter target = FindHelpers.FindByName(Room.People, parameters[0]);
                if (target != null)
                {
                    if (target == this)
                        Send("You like yourself even better!" + Environment.NewLine);
                    else
                    {
                        target.ChangeController(this);
                        Slave = target;
                        Send("{0} looks at you with adoring eyes." + Environment.NewLine, target.Name);
                        target.Send("Isn't {0} so nice?" + Environment.NewLine, Name);
                    }
                }
                else
                    Send(StringHelpers.CharacterNotFound);
            }

            return true;
        }

        [Command("order")]
        protected virtual bool DoOrder(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Order what?" + Environment.NewLine);
            else if (Slave == null)
                Send("You have no followers here." + Environment.NewLine);
            else if (Slave.Room != Room)
                Send(StringHelpers.CharacterNotFound);
            else
            {
                Send("You order {0} to {1}." + Environment.NewLine, Slave.Name, rawParameters);
                Slave.Send("{0} orders you to '{1}'." + Environment.NewLine, Name, rawParameters);
                Slave.ProcessCommand(rawParameters);
            }
            return true;
        }
    }
}
