namespace Mud.Random;

public interface IOccurancy<out T>
{
    T Value { get; }
    int Occurancy { get; }
}
