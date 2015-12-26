namespace Mud.Server
{
    public interface IAdmin : IPlayer
    {
        IEntity Incarnating { get; }
    }
}
