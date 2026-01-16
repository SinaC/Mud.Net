using Mud.Blueprints.Reset;
using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using System.Runtime.Serialization;

namespace Mud.Blueprints.Room;

[DataContract]
public class RoomBlueprint
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public int AreaId { get; set; }

    [DataMember]
    public string Name { get; set; } = default!;

    [DataMember]
    public string Description { get; set; } = default!;

    [DataMember]
    public ExtraDescription[] ExtraDescriptions { get; set; } = []; // keyword -> descriptions

    [DataMember]
    public IRoomFlags RoomFlags { get; set; } = default!;

    [DataMember]
    public SectorTypes SectorType { get; set; } = SectorTypes.City;

    [DataMember]
    public int HealRate { get; set; } = 100;

    [DataMember]
    public int ResourceRate { get; set; } = 100;

    [DataMember]
    public Sizes? MaxSize { get; set; }

    [DataMember]
    public ExitBlueprint[] Exits { get; set; } = default!;

    [DataMember]
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
