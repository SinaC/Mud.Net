namespace Mud.Server
{
    public interface IEquipable : IItem
    {
        ICharacter EquipedBy { get; }

        bool ChangeEquipedBy(ICharacter character);
    }
}
