using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Commands.InGame
{
    [CommandOutOfGame(typeof(ICharacter))]
    public class Kill : IInGameCommand
    {
        public string Name
        {
            get { return "kill"; }
        }

        public string Help
        {
            get { return "TODO"; }
        }

        public bool Execute(IEntity entity, string rawParameters, params CommandParameter[] parameters)
        {
            return true;
        }
    }
}
