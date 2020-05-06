using Mud.Domain;

namespace Mud.Server
{
    public interface IEquipableItem : IItem
    {
        WearLocations WearLocation { get; }

        ICharacter EquipedBy { get; }

        bool ChangeEquipedBy(ICharacter character);
    }
}
