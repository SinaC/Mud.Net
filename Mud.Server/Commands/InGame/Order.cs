namespace Mud.Server.Commands.InGame
{
    public class Order : IInGameCommand
    {
        public string Name
        {
            get { return "order"; }
        }

        public string Help
        {
            get { return "TODO"; }
        }

        public bool Execute(IEntity entity, string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: convert parameters[0] into entity
            // TODO: then convertedEntity.ProcessCommand(rawParameters - first argument   or rebuild parameters without first)

            return true;
        }
    }
}
