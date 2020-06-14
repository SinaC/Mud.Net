namespace Mud.Server.Interfaces.Item
{
    public interface IItemLight : IItem
    {
        bool IsLighten { get; }
        int TimeLeft { get; } // in minutes
        bool IsInfinite { get; }

        bool DecreaseTimeLeft();
    }
}
