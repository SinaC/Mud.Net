using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Aura;
using Mud.Server.Blueprints.Item;

namespace Mud.Server
{
    public interface IItem : IEntity
    {
        IContainer ContainedInto { get; }

        ItemBlueprintBase Blueprint { get; }

        IReadOnlyDictionary<string, string> ExtraDescriptions { get; } // keyword -> description

        int DecayPulseLeft { get; } // 0: means no decay

        int Level { get; }
        int Weight { get; }
        int Cost { get; }
        bool NoTake { get; }

        ItemFlags BaseItemFlags { get; }
        ItemFlags ItemFlags { get; }

        bool IsQuestObjective(IPlayableCharacter questingCharacter);

        bool ChangeContainer(IContainer container);

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
