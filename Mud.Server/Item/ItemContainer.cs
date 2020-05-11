﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemContainer : ItemEquippableBase<ItemContainerBlueprint>, IItemContainer
    {
        private readonly List<IItem> _content;

        public ItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            _content = new List<IItem>();
            MaxWeight = blueprint.MaxWeight;
            ContainerFlags = blueprint.ContainerFlags;
            KeyId = blueprint.Key;
            MaxWeightPerItem = blueprint.MaxWeightPerItem;
            WeightMultiplier = blueprint.WeightMultiplier;
        }

        public ItemContainer(Guid guid, ItemContainerBlueprint blueprint, ItemContainerData itemContainerData, IContainer containedInto)
            : base(guid, blueprint, itemContainerData, containedInto)
        {
            _content = new List<IItem>();
            MaxWeight = blueprint.MaxWeight;
            ContainerFlags = itemContainerData.ContainerFlags;
            KeyId = blueprint.Key;
            MaxWeightPerItem = blueprint.MaxWeightPerItem;
            WeightMultiplier = blueprint.WeightMultiplier;
            if (itemContainerData.Contains?.Length > 0)
            {
                foreach (ItemData itemData in itemContainerData.Contains)
                    World.AddItem(Guid.NewGuid(), itemData, this);
            }
        }

        #region IItemContainer

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

        #region IItem

        public override int Weight => base.Weight + _content.Sum(x => x.Weight) * WeightMultiplier;

        #endregion

        #region IContainer

        public IEnumerable<IItem> Content => _content;

        public int MaxWeight { get; }
        public int MaxWeightPerItem { get; }

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

        public ContainerFlags ContainerFlags { get; protected set; }
        public int WeightMultiplier { get; } // percentage

        #endregion

        #region ItemBase

        void IEntity.OnRemoved()
        {
            _content.Clear();
        }

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
