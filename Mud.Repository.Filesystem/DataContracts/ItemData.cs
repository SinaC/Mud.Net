using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    [KnownType(typeof(ItemContainerData))]
    [KnownType(typeof(ItemCorpseData))]
    [XmlInclude(typeof(ItemCorpseData))]
    [XmlInclude(typeof(ItemContainerData))]
    public class ItemData
    {
        [DataMember]
        public int ItemId { get; set; }

        [DataMember]
        public int DecayPulseLeft { get; set; }

    }
}
