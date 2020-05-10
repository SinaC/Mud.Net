﻿using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;

namespace Mud.Server.Item
{
    public class ItemPortal : ItemBase<ItemPortalBlueprint>, IItemPortal
    {
        public const int InfiniteChargeCount = -1;
        public const int NoDestinationRoomId = -1;

        public ItemPortal(Guid guid, ItemPortalBlueprint blueprint, IRoom destination, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Destination = destination;
            PortalFlags = blueprint.PortalFlags;
            MaxChargeCount = blueprint.MaxChargeCount;
            CurrentChargeCount = blueprint.CurrentChargeCount;
        }

        public ItemPortal(Guid guid, ItemPortalBlueprint blueprint, ItemPortalData itemData, IRoom destination, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            Destination = destination;
            PortalFlags = itemData.PortalFlags;
            MaxChargeCount = itemData.MaxChargeCount;
            CurrentChargeCount = itemData.CurrentChargeCount;
        }

        #region IItemPortal

        public IRoom Destination { get; protected set; }
        
        public PortalFlags PortalFlags { get; protected set; }

        public int MaxChargeCount { get; protected set; }

        public int CurrentChargeCount { get; protected set; }

        public bool IsClosed => (PortalFlags & PortalFlags.Closed) == PortalFlags.Closed;
        public bool IsLocked => (PortalFlags & PortalFlags.Locked) == PortalFlags.Locked;

        public void Open()
        {
            RemoveFlags(PortalFlags.Closed);
        }

        public void Close()
        {
            AddFlags(PortalFlags.Closed);
        }

        public void Unlock()
        {
            RemoveFlags(PortalFlags.Locked);
        }

        public void Lock()
        {
            AddFlags(PortalFlags.Locked);
        }

        public bool HasChargeLeft()
        {
            if (MaxChargeCount == InfiniteChargeCount)
                return true;
            return CurrentChargeCount > 0;
        }

        public void ChangeDestination(IRoom destination)
        {
            Destination = destination;
        }

        public void Use()
        {
            if (MaxChargeCount == InfiniteChargeCount)
                return;
            CurrentChargeCount = Math.Max(0, CurrentChargeCount - 1);
        }

        public void SetCharge(int current, int max)
        {
            MaxChargeCount = Math.Max(1, max);
            CurrentChargeCount = current.Range(1, MaxChargeCount);
        }

        #endregion

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemPortalData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags, // Current will be recompute with auras
                Auras = MapAuraData(),
                DestinationRoomId = Destination?.Blueprint?.Id ?? -1,
                PortalFlags = PortalFlags,
                MaxChargeCount = MaxChargeCount,
                CurrentChargeCount = CurrentChargeCount,
            };
        }

        #endregion

        private void AddFlags(PortalFlags flags)
        {
            PortalFlags |= flags;
        }

        private void RemoveFlags(PortalFlags flags)
        {
            PortalFlags &= ~flags;
        }
    }
}
