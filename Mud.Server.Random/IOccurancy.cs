namespace Mud.Server.Random
{
    public interface IOccurancy<out T>
    {
        T Value { get; }
        int Occurancy { get; }
    }
}
