﻿using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;
using System;
using System.Collections.Generic;

namespace Mud.Server.Rom24.Flags
{
    public class ItemFlagValues : FlagValuesBase<string>, IItemFlagValues
    {
        private static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "None",
            "Glowing",
            "Humming",
            "Dark",
            "Lock",
            "Evil",
            "Invis",
            "Magic",
            "NoDrop", // Cannot be dropped once in inventory (cannot be put in container) [can be uncursed]
            "Bless",
            "AntiGood",
            "AntiEvil",
            "AntiNeutral",
            "NoRemove", // Cannot be removed once equipped [can be uncursed]
            "Inventory",
            "NoPurge",
            "RotDeath", // Disappear when holder dies
            "VisibleDeath", // Visible when holder dies
            "NonMetal",
            "NoLocate",
            "MeltOnDrop", // Melt when dropped
            "HadTimer",
            "SellExtract",
            "BurnProof",
            "NoUncurse",
            "NoSacrifice",
        };

        protected override HashSet<string> HashSet => Flags;

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            Log.Default.WriteLine(LogLevels.Error, $"Item flags '{string.Join(",", values)}' not found in {GetType().FullName}");
        }
    }
}
