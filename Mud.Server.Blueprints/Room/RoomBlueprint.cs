using Mud.Domain;
using Mud.Server.Blueprints.Reset;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Mud.Server.Blueprints.Room
{
    [DataContract]
    public class RoomBlueprint
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int AreaId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Lookup<string, string> ExtraDescriptions { get; set; } // keyword -> descriptions

        [DataMember]
        public RoomFlags RoomFlags { get; set; } = RoomFlags.None;

        [DataMember]
        public SectorTypes SectorType { get; set; } = SectorTypes.City;

        [DataMember]
        public int HealRate { get; set; } = 100;

        [DataMember]
        public int ResourceRate { get; set; } = 100;

        [DataMember]
        public Sizes? MaxSize { get; set; } = null;

        [DataMember]
        public ExitBlueprint[] Exits { get; set; }

        [DataMember]
        public List<ResetBase> Resets { get; set; } = new List<ResetBase>();

        public static Lookup<string, string> BuildExtraDescriptions(IEnumerable<KeyValuePair<string, string>> extraDescriptions)
        {
            return (Lookup<string, string>)extraDescriptions.SelectMany(x => x.Key.Split(' '), (kv, key) => new { key, desc = kv.Value }).ToLookup(x => x.key, x => x.desc);
        }
    }
}
