using Mud.Blueprints;
using Mud.Blueprints.Item;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using System.Text;

namespace Mud.Server.Interfaces.Item;

public interface IItem : IEntity
{
    void Initialize<TBlueprint>(Guid guid, TBlueprint blueprint, IContainer containedInto)
       where TBlueprint : ItemBlueprintBase;
    void Initialize<TBlueprint, TData>(Guid guid, TBlueprint blueprint, TData data, string name, string shortDescription, string description, IContainer containedInto)
        where TBlueprint : ItemBlueprintBase
        where TData : ItemData;
    void Initialize<TBlueprint, TData>(Guid guid, TBlueprint blueprint, TData data, IContainer containedInto)
        where TBlueprint : ItemBlueprintBase
        where TData : ItemData;

    IContainer ContainedInto { get; }

    ItemBlueprintBase Blueprint { get; }

    IEnumerable<ExtraDescription> ExtraDescriptions { get; }

    WearLocations WearLocation { get; }
    ICharacter? EquippedBy { get; }

    int DecayPulseLeft { get; } // 0: means no decay

    int Level { get; }
    int Weight { get; }
    int Cost { get; }
    bool NoTake { get; }
    int TotalWeight { get; }
    int CarryCount { get; }

    IItemFlags BaseItemFlags { get; }
    IItemFlags ItemFlags { get; }

    bool IsQuestObjective(IPlayableCharacter questingCharacter);

    bool ChangeContainer(IContainer? container);

    bool ChangeEquippedBy(ICharacter? character, bool recompute);

    void DecreaseDecayPulseLeft(int pulseCount);
    void SetTimer(TimeSpan duration);

    void AddBaseItemFlags(bool recompute, params string[] flags);
    void RemoveBaseItemFlags(bool recompute, params string[] flags);
    void Disenchant();

    void IncreaseLevel();

    StringBuilder Append(StringBuilder sb, ICharacter viewer, bool shortDisplay);

    // Affects
    void ApplyAffect(IItemFlagsAffect affect);

    // Mapping
    ItemData MapItemData();
}
