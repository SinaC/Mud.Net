using System;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemEquippableBase<TBlueprint> : ItemBase<TBlueprint>, IEquippableItem
        where TBlueprint : ItemBlueprintBase
    {
        public ItemEquippableBase(Guid guid, TBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            WearLocation = blueprint.WearLocation;
        }

        public ItemEquippableBase(Guid guid, TBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            WearLocation = blueprint.WearLocation;
        }

        #region IEquippableItem

        #region IItem

        public override void Recompute()
        {
            // Don't call Base.Recompute because this method is a copy/paste with equipped step added

            // 0) Reset
            ResetAttributes();

            // 1) Apply auras from room containing item if in a room
            if (ContainedInto is IRoom room && room.IsValid)
            {
                ApplyAuras<IItem>(room, this);
            }

            // 2) Apply auras from character equiping item if equipped by a character
            if (EquippedBy != null && EquippedBy.IsValid)
            {
                ApplyAuras<IItem>(EquippedBy, this);
            }

            // 3) Apply own auras
            ApplyAuras<IItem>(this, this);
        }

        #endregion

        public WearLocations WearLocation { get; }

        public ICharacter EquippedBy { get; private set; }

        public bool ChangeEquippedBy(ICharacter character)
        {
            EquippedBy?.Unequip(this);
            Log.Default.WriteLine(LogLevels.Info, "ChangeEquippedBy: {0} : {1} -> {2}", DebugName, EquippedBy?.DebugName ?? "<<??>>", character?.DebugName ?? "<<??>>");
            EquippedBy = character;
            // TODO: call something like character.Equip ? (additional parameter EquipmentSlot)
            return true;
        }

        #endregion
    }
}
