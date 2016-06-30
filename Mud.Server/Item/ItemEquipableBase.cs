using System;
using Mud.Logger;
using Mud.Server.Blueprints;
using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public class ItemEquipableBase : ItemBase, IEquipable
    {
        public ItemEquipableBase(Guid guid, ItemBlueprintBase blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            WearLocation = blueprint.WearLocation;
        }

        public WearLocations WearLocation { get; }

        public ICharacter EquipedBy { get; private set; }

        public bool ChangeEquipedBy(ICharacter character)
        {
            EquipedBy?.Unequip(this);
            Log.Default.WriteLine(LogLevels.Info, "ChangeEquipedBy: {0} : {1} -> {2}", Name, EquipedBy == null ? "<<??>>" : EquipedBy.Name, character == null ? "<<??>>" : character.Name);
            EquipedBy = character;
            // TODO: call something like character.Equip ? (additional parameter EquipmentSlot)
            return true;
        }
    }
}
