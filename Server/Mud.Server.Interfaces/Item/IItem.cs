using Mud.Blueprints;
using Mud.Blueprints.Item;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Interfaces.Item;

public interface IItem : IEntity
{
    void Initialize<TBlueprint>(Guid guid, TBlueprint blueprint, string source, IContainer containedInto)
       where TBlueprint : ItemBlueprintBase;
    void Initialize<TBlueprint, TData>(Guid guid, TBlueprint blueprint, TData data, string name, string shortDescription, string description, IContainer containedInto)
        where TBlueprint : ItemBlueprintBase
        where TData : ItemData;
    void Initialize<TBlueprint, TData>(Guid guid, TBlueprint blueprint, TData data, IContainer containedInto)
        where TBlueprint : ItemBlueprintBase
        where TData : ItemData;

    IContainer ContainedInto { get; }

    ItemBlueprintBase Blueprint { get; }

    string Source { get; }

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

    bool IsQuestObjective(IPlayableCharacter questingCharacter, bool checkCompleted);

    bool ChangeContainer(IContainer? container);

    bool ChangeEquippedBy(ICharacter? character, bool recompute);

    void DecreaseDecayPulseLeft(int pulseCount);
    void SetTimer(TimeSpan duration);

    void AddBaseItemFlags(bool recompute, params string[] flags);
    void RemoveBaseItemFlags(bool recompute, params string[] flags);
    void Disenchant();

    //
    void SetShortDescription(string shortDescription);

    //
    void IncreaseLevel();
    void SetLevel(int level);
    //
    void SetCost(int cost);

    StringBuilder Append(StringBuilder sb, ICharacter viewer, bool shortDisplay);

    // Affects
    void ApplyAffect(IItemFlagsAffect affect);

    void OnRemoved(IRoom nullRoom);

    // Mapping
    ItemData MapItemData();
}