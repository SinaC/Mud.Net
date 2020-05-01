using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class CharacterSexAffectData : AffectDataBase
    {
        [DataMember]
        public int Value { get; set; }
    }
}
