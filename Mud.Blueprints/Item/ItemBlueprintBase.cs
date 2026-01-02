using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using System.Runtime.Serialization;

namespace Mud.Blueprints.Item;

[DataContract]
[KnownType(typeof(ItemArmorBlueprint))]
[KnownType(typeof(ItemContainerBlueprint))]
[KnownType(typeof(ItemCorpseBlueprint))]
[KnownType(typeof(ItemFurnitureBlueprint))]
[KnownType(typeof(ItemJewelryBlueprint))]
[KnownType(typeof(ItemKeyBlueprint))]
[KnownType(typeof(ItemLightBlueprint))]
[KnownType(typeof(ItemPortalBlueprint))]
[KnownType(typeof(ItemQuestBlueprint))]
[KnownType(typeof(ItemShieldBlueprint))]
[KnownType(typeof(ItemWeaponBlueprint))]
public abstract class ItemBlueprintBase
{
    [DataMember]
    public int Id { get; set; }
    
    [DataMember]
    public string Name { get; set; } = default!;

    [DataMember]
    public string ShortDescription { get; set; } = default!;

    [DataMember]
    public string Description { get; set; } = default!;

    [DataMember]
    public int Level { get; set; }

    [DataMember]
    public int Weight { get; set; }

    [DataMember]
    public int Cost { get; set; }

    [DataMember]
    public bool NoTake { get; set; }

    [DataMember]
    public ExtraDescription[] ExtraDescriptions { get; set; } = []; // keywords (separated with blank space) -> description

    // TODO: flags, level, ...

    [DataMember]
    public WearLocations WearLocation { get; set; }

    [DataMember]
    public IItemFlags ItemFlags { get; set; } = default!;

    public static ExtraDescription[] BuildExtraDescriptions(IEnumerable<KeyValuePair<string, string>> extraDescriptions)
    {
        return extraDescriptions.Select(x => new ExtraDescription
        {
            Keywords = x.Key.Split(' ', StringSplitOptions.RemoveEmptyEntries),
            Description = x.Value
        }).ToArray();
    }

    public bool Equals(ItemBlueprintBase? other)
    {
        return other != null && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ItemBlueprintBase);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
