﻿using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [Command("order", Category = "Group")]
        [Syntax("[cmd] <command>")]
        protected virtual bool DoOrder(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Order what?");
                return true;
            }
            if (Slave == null)
            {
                Send("You have no followers here.");
                return true;
            }
            if (Slave.Room != Room)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            Slave.Send("{0} orders you to '{1}'.", DisplayName, rawParameters);
            Slave.ProcessCommand(rawParameters);
            if (this is IPlayableCharacter playableCharacter)
                playableCharacter.ImpersonatedBy?.SetGlobalCooldown(3);
            //Send("You order {0} to {1}.", Slave.Name, rawParameters);
            Send("Ok.");
            return true;
        }
    }
}
