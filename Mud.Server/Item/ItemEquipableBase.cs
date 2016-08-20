using System;
using Mud.Logger;
using Mud.Server.Blueprints;
using Mud.Server.Blueprints.Item;
using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public class ItemEquipableBase<TBlueprint> : ItemBase<TBlueprint>, IEquipable
        where TBlueprint : ItemBlueprintBase
    {
        public ItemEquipableBase(Guid guid, TBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            WearLocation = blueprint.WearLocation;
        }

        public WearLocations WearLocation { get; }

        public ICharacter EquipedBy { get; private set; }

        public bool ChangeEquipedBy(ICharacter character)
        {
            EquipedBy?.Unequip(this);
            Log.Default.WriteLine(LogLevels.Info, "ChangeEquipedBy: {0} : {1} -> {2}", DebugName, EquipedBy == null ? "<<??>>" : EquipedBy.DebugName, character == null ? "<<??>>" : character.DebugName);
            EquipedBy = character;
            // TODO: call something like character.Equip ? (additional parameter EquipmentSlot)
            return true;
        }
    }
}
