namespace Mud.Server.Interfaces.Item;

public interface IItemArmor : IItem
{
    int Bash { get; }
    int Pierce { get; }
    int Slash { get; }
    int Exotic { get; }

    void SetArmor(int bash, int pierce, int slash, int exotic);
}
