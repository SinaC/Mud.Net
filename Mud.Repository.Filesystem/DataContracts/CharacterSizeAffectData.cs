using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class CharacterSizeAffectData : AffectDataBase
    {
        [DataMember]
        public int Value { get; set; }
    }
}
