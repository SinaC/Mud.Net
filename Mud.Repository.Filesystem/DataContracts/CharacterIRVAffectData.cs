using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class CharacterIRVAffectData : AffectDataBase
    {
        [DataMember]
        public int Location { get; set; }

        [DataMember]
        public int Operator { get; set; } // Add and Or are identical

        [DataMember]
        public int Modifier { get; set; }
    }
}
