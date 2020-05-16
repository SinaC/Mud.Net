using Mud.Domain;
using Mud.Server.Blueprints.Reset;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Server.Blueprints.Room
{
    [DataContract]
    public class RoomBlueprint
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Dictionary<string, string> ExtraDescriptions { get; set; } // keyword -> description

        [DataMember]
        public RoomFlags RoomFlags { get; set; }

        [DataMember]
        public ExitBlueprint[] Exits { get; set; } // TODO: fixed length or list (+ add direction in ExitBlueprint)

        // TODO: healrate, sector, ...

        [DataMember]
        public List<ResetBase> Resets { get; set; } = new List<ResetBase>();

        public static Dictionary<string, string> BuildExtraDescriptions(IEnumerable<KeyValuePair<string, string>> extraDescriptions)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (extraDescriptions == null)
                return result;
            foreach (KeyValuePair<string, string> kv in extraDescriptions)
            {
                foreach (string keyword in kv.Key.Split(' '))
                    result.Add(keyword, kv.Value);
            }
            return result;
            ;
        }
    }
}
