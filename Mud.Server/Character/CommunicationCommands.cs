using System;
using System.Linq;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("say")]
        protected virtual bool DoSay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Say what?");
            else
            {
                Send("%g%You say '%x%{0}%g%'%x%", rawParameters);
                string message = String.Format("%g%{0} says '%x%{1}%g%'%x%", Name, rawParameters);
                foreach (ICharacter character in Room.CharactersInRoom.Where(x => x != this))
                    character.Send(message);
            }
            return true;
        }

        [Command("yell")]
        protected virtual bool DoYell(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Yell what?");
            else
            {
                // TODO: say to everyone in area (or in specific range)
            }
            return true;
        }
    }
}
