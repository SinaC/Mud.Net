using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemContainer : ItemEquipableBase<ItemContainerBlueprint>, IItemContainer
    {
        private readonly List<IItem> _content;

        public ItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            _content = new List<IItem>();
            ItemCount = blueprint.ItemCount;
            WeightMultiplier = blueprint.WeightMultiplier;
            KeyId = blueprint.Key;
            ContainerFlags = blueprint.ContainerFlags;
        }

        public ItemContainer(Guid guid, ItemContainerBlueprint blueprint, ItemContainerData itemContainerData, IContainer containedInto)
            : base(guid, blueprint, itemContainerData, containedInto)
        {
            _content = new List<IItem>();
            // TODO: key
            ItemCount = blueprint.ItemCount;
            WeightMultiplier = blueprint.WeightMultiplier;
            KeyId = blueprint.Key;
            ContainerFlags = itemContainerData.ContainerFlags;
            if (itemContainerData.Contains?.Length > 0)
            {
                foreach (ItemData itemData in itemContainerData.Contains)
                    World.AddItem(Guid.NewGuid(), itemData, this);
            }
        }

        #region IItem

        #region IItemCloseable

        public int KeyId { get; }

        public bool IsCloseable => !ContainerFlags.HasFlag(ContainerFlags.NoClose);
        public bool IsLockable => !ContainerFlags.HasFlag(ContainerFlags.NoLock) && KeyId > 0;
        public bool IsClosed => ContainerFlags.HasFlag(ContainerFlags.Closed);
        public bool IsLocked => ContainerFlags.HasFlag(ContainerFlags.Locked);
        public bool IsPickProof => ContainerFlags.HasFlag(ContainerFlags.PickProof);
        public bool IsEasy => ContainerFlags.HasFlag(ContainerFlags.Easy);
        public bool IsHard => ContainerFlags.HasFlag(ContainerFlags.Hard);

        public void Open()
        {
            RemoveFlags(ContainerFlags.Closed);
        }

        public void Close()
        {
            if (IsCloseable)
                AddFlags(ContainerFlags.Closed);
        }

        public void Unlock()
        {
            RemoveFlags(ContainerFlags.Locked);
        }

        public void Lock()
        {
            if (IsLockable && IsClosed)
                AddFlags(ContainerFlags.Locked);
        }

        #endregion

        public override int Weight => base.Weight + _content.Sum(x => x.Weight)*WeightMultiplier;

        void IEntity.OnRemoved()
        {
            _content.Clear();
        }

        #endregion

        #region IItemContainer

        public int ItemCount { get; } // maximum number of items
        public int WeightMultiplier { get; } // percentage
        public ContainerFlags ContainerFlags { get; protected set; }

        #region IContainer

        public IEnumerable<IItem> Content => _content;

        public bool PutInContainer(IItem obj)
        {
            // TODO: check if already in a container, check max items
            _content.Insert(0, obj);
            return true;
        }

        public bool GetFromContainer(IItem obj)
        {
            bool removed = _content.Remove(obj);
            return removed;
        }

        #endregion

        #endregion

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemContainerData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags,
                Auras = MapAuraData(),
                ContainerFlags = ContainerFlags,
                Contains = MapContent(),
            };
        }

        #endregion

        private void AddFlags(ContainerFlags flags)
        {
            ContainerFlags |= flags;
        }

        private void RemoveFlags(ContainerFlags flags)
        {
            ContainerFlags &= ~flags;
        }

        private ItemData[] MapContent()
        {
            if (Content.Any())
                return Content.Select(x => x.MapItemData()).ToArray();
            return null;
        }
    }
}
