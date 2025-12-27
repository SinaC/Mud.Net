namespace Mud.Server.Interfaces
{
    public interface IPulseManager
    {
        IEnumerable<string> PulseNames { get; }

        void Add(string name, int initialValue, int resetValue, Action<int> action);
        void Pulse();
        void Pulse(string name);
        void Clear();
    }
}
