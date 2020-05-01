using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class CharacterAttributeAffectData : AffectDataBase
    {
        [DataMember]
        public int Operator { get; set; } // Or and Nor cannot be used

        [DataMember]
        public int Location { get; set; }

        [DataMember]
        public int Modifier { get; set; }
    }
}
