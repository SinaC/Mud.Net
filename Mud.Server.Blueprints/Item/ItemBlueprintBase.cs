using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Mud.Server.Constants;

namespace Mud.Server.Blueprints.Item
{
    [DataContract]
    public abstract class ItemBlueprintBase
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public string ShortDescription { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Weight { get; set; }

        [DataMember]
        public int Cost { get; set; }

        [DataMember]
        public Dictionary<string, string> ExtraDescriptions { get; set; } // keyword -> description

        // TODO: flags, level, ...

        [DataMember]
        public WearLocations WearLocation { get; set; }

        public static Dictionary<string,string> BuildExtraDescriptions(IEnumerable<KeyValuePair<string, string>> extraDescriptions)
        {
            Dictionary<string,string> result = new Dictionary<string, string>();
            if (extraDescriptions == null)
                return result;
            foreach (KeyValuePair<string, string> kv in extraDescriptions)
            {
                foreach(string keyword in kv.Key.Split(' '))
                    result.Add(keyword, kv.Value);
            }
            return result;
            ;
        }
    }
}
