using System;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemEquipableBase<TBlueprint> : ItemBase<TBlueprint>, IEquipableItem
        where TBlueprint : ItemBlueprintBase
    {
        public ItemEquipableBase(Guid guid, TBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            WearLocation = blueprint.WearLocation;
        }

        public ItemEquipableBase(Guid guid, TBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            WearLocation = blueprint.WearLocation;
        }

        #region IEquipable

        #region IITem

        public override void Recompute()
        {
            // Don't call Base.Recompute because this method is a copy/paste with equiped step added

            // 0) Reset
            ResetAttributes();

            // 1) Apply auras from room containing item if in a room
            if (ContainedInto is IRoom room && room.IsValid)
            {
                ApplyAuras<IItem>(room, this);
            }

            // 2) Apply auras from character equiping item if equiped by a character
            if (EquipedBy != null && EquipedBy.IsValid)
            {
                ApplyAuras<IItem>(EquipedBy, this);
            }

            // 3) Apply own auras
            ApplyAuras<IItem>(this, this);
        }

        #endregion

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

        #endregion
    }
}
