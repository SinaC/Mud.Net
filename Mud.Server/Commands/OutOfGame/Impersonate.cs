namespace Mud.Server.Commands.OutOfGame
{
    public class Impersonate : IOutOfGameCommand
    {
        public string Name
        {
            get { return "impersonate"; }
        }

        public string Help { get { return "TODO"; } }

        public bool Execute(IPlayer player, string rawParameters, params CommandParameter[] parameters)
        {
            // non-admin player can only impersonate their avatar (ICharacter)
            // admin player can impersonate everything
            return true;
        }
    }
}
