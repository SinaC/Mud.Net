namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IItemArmor : IItem
    {
        int Bash { get; }
        int Pierce { get; }
        int Slash { get; }
        int Exotic { get; }
    }
}
