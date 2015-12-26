namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("charm")]
        protected virtual bool Charm(string rawParameters, CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Slave != null)
                {
                    Send("You stop controlling {0}", Slave.Name);
                    Slave.ChangeController(null);
                    Slave = null;
                }
                else
                    Send("Try controlling something before trying to un-control");
            }
            else
            {
                //TODO: must be in same room
                ICharacter target = WorldTest.Instance.GetCharacter(parameters[0]);
                if (target != null)
                {
                    Send("You start controlling {0}", target.Name);
                    target.ChangeController(this);
                    Slave = target;
                }
                else
                    Send("Target not found");
            }

            return true;
        }

        [Command("order")]
        protected virtual bool Order(string rawParameters, CommandParameter[] parameters)
        {
            if (Slave == null)
                Send("Try controlling something before giving orders");
            else
            {
                string orderCommand;
                string orderRawParameters;
                CommandParameter[] orderParameters;
                bool orderForceOutOfGame;

                bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(rawParameters, out orderCommand, out orderRawParameters, out orderParameters, out orderForceOutOfGame);
                if (!extractedSuccessfully)
                    Send("Problem order arguments");
                else
                {
                    Send("You order to {0} to {1}", Slave.Name, rawParameters);
                    Slave.ExecuteCommand(orderCommand, orderRawParameters, orderParameters);
                }
            }
            return true;
        }
    }
}
