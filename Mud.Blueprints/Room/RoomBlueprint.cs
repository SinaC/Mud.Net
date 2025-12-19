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
    public Lookup<string, string> ExtraDescriptions { get; set; } = default!; // keyword -> descriptions

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

    public static Lookup<string, string> BuildExtraDescriptions(IEnumerable<KeyValuePair<string, string>> extraDescriptions)
    {
        return (Lookup<string, string>)extraDescriptions.SelectMany(x => x.Key.Split(' '), (kv, key) => new { key, desc = kv.Value }).ToLookup(x => x.key, x => x.desc);
    }
}
