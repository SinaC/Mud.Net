namespace Mud.Server.Commands.InGame
{
    // Syntax:
    //  look: look at environment
    //  look <obj>: look at object in character's inventory or environment's inventory
    public class Look : IInGameCommand
    {
        public string Name
        {
            get { return "look"; }
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
