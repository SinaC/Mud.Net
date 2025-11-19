namespace Mud.Server.Interfaces
{
    public interface IPulseManager
    {
        void Add(string name, int initialValue, int resetValue, Action<int> action);
        void Pulse();
        void Clear();
    }
}
