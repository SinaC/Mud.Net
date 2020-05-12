using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    [KnownType(typeof(CharacterAttributeAffectData))]
    [KnownType(typeof(CharacterFlagsAffectData))]
    [KnownType(typeof(CharacterIRVAffectData))]
    [KnownType(typeof(CharacterSexAffectData))]
    [KnownType(typeof(ItemFlagsAffectData))]
    [KnownType(typeof(ItemWeaponFlagsAffectData))]
    [KnownType(typeof(CharacterSizeAffectData))]
    [XmlInclude(typeof(CharacterAttributeAffectData))]
    [XmlInclude(typeof(CharacterFlagsAffectData))]
    [XmlInclude(typeof(CharacterIRVAffectData))]
    [XmlInclude(typeof(CharacterSexAffectData))]
    [XmlInclude(typeof(ItemFlagsAffectData))]
    [XmlInclude(typeof(ItemWeaponFlagsAffectData))]
    [XmlInclude(typeof(CharacterSizeAffectData))]
    public abstract class AffectDataBase
    {
    }
}
