using Mud.Server.Commands;

namespace Mud.Server.Old.Commands.InGame
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
