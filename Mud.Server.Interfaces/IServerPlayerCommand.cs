namespace Mud.Server
{
    public interface IServerPlayerCommand
    {
        void Quit(IPlayer player);
        void Delete(IPlayer player);

    }
}
