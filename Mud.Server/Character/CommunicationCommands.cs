namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("say")]
        protected virtual bool Say(string rawParameters, CommandParameter[] parameters)
        {
            // TODO: say to everyone in the room
            return true;
        }

        [Command("yell")]
        protected virtual bool Yell(string rawParameters, CommandParameter[] parameters)
        {
            // TODO: say to everyone in area (or in specific range)
            return true;
        }
    }
}
