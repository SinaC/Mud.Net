using System;
using System.Linq;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("say", Category = "Communication")]
        protected virtual bool DoSay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Say what?" + Environment.NewLine);
            else
            {
                Send("%g%You say '%x%{0}%g%'%x%" + Environment.NewLine, rawParameters);
                string message = String.Format("%g%{0} says '%x%{1}%g%'%x%" + Environment.NewLine, DisplayName, rawParameters);
                foreach (ICharacter character in Room.People.Where(x => x != this))
                    character.Send(message);
            }
            return true;
        }

        [Command("yell", Category = "Communication")]
        protected virtual bool DoYell(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Yell what?" + Environment.NewLine);
            else
            {
                // TODO: say to everyone in area (or in specific range)
                Send(StringHelpers.NotYetImplemented);
            }
            return true;
        }
    }
}
