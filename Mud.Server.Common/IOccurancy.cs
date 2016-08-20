namespace Mud.Server.Common
{
    public interface IOccurancy<out T>
    {
        T Value { get; }
        int Occurancy { get; }
    }
}
