using Mud.Domain;

namespace Mud.Server
{
    public interface IEquippableItem : IItem
    {
        WearLocations WearLocation { get; }

        ICharacter EquippedBy { get; }

        bool ChangeEquippedBy(ICharacter character);
    }
}
