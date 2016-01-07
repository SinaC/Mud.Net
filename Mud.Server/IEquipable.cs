using Mud.Server.Constants;

namespace Mud.Server
{
    public interface IEquipable : IItem
    {
        WearLocations WearLocation { get; }

        ICharacter EquipedBy { get; }

        bool ChangeEquipedBy(ICharacter character);
    }
}
