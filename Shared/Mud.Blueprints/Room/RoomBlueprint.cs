using Mud.Blueprints.Reset;
using Mud.Domain;
using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Room;

public class RoomBlueprint
{
    public int Id { get; set; }

    public int AreaId { get; set; }

    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;

    public ExtraDescription[] ExtraDescriptions { get; set; } = []; // keyword -> descriptions

    public IRoomFlags RoomFlags { get; set; } = default!;

    public SectorTypes SectorType { get; set; } = SectorTypes.City;

    public int HealRate { get; set; } = 100;

    public int ResourceRate { get; set; } = 100;

    public Sizes? MaxSize { get; set; }

    public ExitBlueprint[] Exits { get; set; } = default!;

    public List<ResetBase> Resets { get; set; } = [];

    public static ExtraDescription[] BuildExtraDescriptions(IEnumerable<KeyValuePair<string, string>> extraDescriptions)
    {
        return extraDescriptions.Select(x => new ExtraDescription
        {
            Keywords = x.Key.Split(' ', StringSplitOptions.RemoveEmptyEntries),
            Description = x.Value
        }).ToArray();
    }
}
