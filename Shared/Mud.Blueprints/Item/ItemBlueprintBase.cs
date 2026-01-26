using Mud.Blueprints.Item.Affects;
using Mud.Domain;
using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Item;

public abstract class ItemBlueprintBase
{
    public int Id { get; set; }
    
    public string Name { get; set; } = default!;

    public string ShortDescription { get; set; } = default!;

    public string Description { get; set; } = default!;

    public int Level { get; set; }

    public int Weight { get; set; }

    public int Cost { get; set; }

    public bool NoTake { get; set; }

    public ExtraDescription[] ExtraDescriptions { get; set; } = []; // keywords (separated with blank space) -> description

    // TODO: flags, level, ...

    public WearLocations WearLocation { get; set; }

    public IItemFlags ItemFlags { get; set; } = default!;

    public ItemAffectBase[] ItemAffects { get; set; } = [];

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
