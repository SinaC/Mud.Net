﻿namespace Mud.Domain
{
    public class ItemCastSpellsChargeData : ItemData
    {
        public int MaxChargeCount { get; set; }
        public int CurrentChargeCount { get; set; }
        public bool AlreadyRecharged { get; set; }
    }
}
