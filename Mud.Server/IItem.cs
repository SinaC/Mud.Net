﻿using System;
using System.Linq;
using Mud.Domain;
using Mud.Server.Aura;
using Mud.Server.Blueprints.Item;

namespace Mud.Server
{
    public interface IItem : IEntity
    {
        IContainer ContainedInto { get; }

        ItemBlueprintBase Blueprint { get; }

        ILookup<string, string> ExtraDescriptions { get; } // keyword -> descriptions

        WearLocations WearLocation { get; }
        ICharacter EquippedBy { get; }

        int DecayPulseLeft { get; } // 0: means no decay

        int Level { get; }
        int Weight { get; }
        int Cost { get; }
        bool NoTake { get; }
        int TotalWeight { get; }
        int CarryCount { get; }

        ItemFlags BaseItemFlags { get; }
        ItemFlags ItemFlags { get; }

        bool IsQuestObjective(IPlayableCharacter questingCharacter);

        bool ChangeContainer(IContainer container);

        bool ChangeEquippedBy(ICharacter character, bool recompute);

        void DecreaseDecayPulseLeft(int pulseCount);
        void SetTimer(TimeSpan duration);

        void AddBaseItemFlags(ItemFlags itemFlags);
        void RemoveBaseItemFlags(ItemFlags itemFlags);

        void IncreaseLevel();

        // Affects
        void ApplyAffect(ItemFlagsAffect affect);

        // Mapping
        ItemData MapItemData();
    }
}
