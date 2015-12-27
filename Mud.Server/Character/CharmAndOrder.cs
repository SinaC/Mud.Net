using System;
using System.Linq;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("charm")]
        protected virtual bool Charm(string rawParameters, CommandParameter[] parameters)
        {
            if (ControlledBy != null)
                Send("You feel like taking, not giving, orders.");
            else if (parameters.Length == 0)
            {
                if (Slave != null)
                {
                    Send("You stop controlling {0}.", Slave.Name);
                    Slave.ChangeController(null);
                    Slave = null;
                }
                else
                    Send("Try controlling something before trying to un-control.");
            }
            else
            {
                ICharacter target = FindHelpers.FindByName(Room.CharactersInRoom, parameters[0]);
                if (target != null)
                {
                    if (target == this)
                        Send("You like yourself even better!");
                    else
                    {
                        target.ChangeController(this);
                        Slave = target;
                        Send("{0} looks at you with adoring eyes.", target.Name);
                        target.Send("Isn't {0} so nice?", Name);
                    }
                }
                else
                    Send(MessageConstants.CharacterNotFound);
            }

            return true;
        }

        [Command("order")]
        protected virtual bool Order(string rawParameters, CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Order what?");
            else if (Slave == null)
                Send("You have no followers here.");
            else if (Slave.Room != Room)
                Send(MessageConstants.CharacterNotFound);
            else
            {
                Send("You order {0} to {1}.", Slave.Name, rawParameters);
                Slave.Send("{0} orders you to '{1}'.", Name, rawParameters);
                Slave.ProcessCommand(rawParameters);
            }
            return true;
        }
    }
}
