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
            // Usage: impersonate <vnum>: impersonate character matching vnum
            // Usage: impersonate <name>: impersonate character matching name
            // Usage: impersonate <vnum> object|room: impersonate object|room matching vnum
            // Usage: impersonate <name> object|room: impersonate object|room matching vnum
            return true;
        }
    }
}
