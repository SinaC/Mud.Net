using System;
using System.Linq;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("say")]
        protected virtual bool DoSay(string rawParameters, CommandParameter[] parameters)
        {
            Send("You say '{0}'", rawParameters);
            string message = String.Format("{0} says '{1}'", Name, rawParameters);
            foreach (ICharacter character in Room.CharactersInRoom.Where(x => x != this))
                character.Send(message);
            return true;
        }

        [Command("yell")]
        protected virtual bool DoYell(string rawParameters, CommandParameter[] parameters)
        {
            // TODO: say to everyone in area (or in specific range)
            return true;
        }
    }
}
