﻿using Mud.Domain;

namespace Mud.Server.Item
{
    public interface IItemPortal : IItemCloseable
    {
        IRoom Destination { get; }
        PortalFlags PortalFlags { get; }
        int MaxChargeCount { get; }
        int CurrentChargeCount { get; }

        bool HasChargeLeft();
        void ChangeDestination(IRoom destination);
        void Use();
        void SetCharge(int current, int max);
    }
}
