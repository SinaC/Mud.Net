using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.Domain
{
    [XmlInclude(typeof(CharacterAttributeAffectData))]
    [XmlInclude(typeof(CharacterFlagsAffectData))]
    [XmlInclude(typeof(CharacterIRVAffectData))]
    [XmlInclude(typeof(CharacterSexAffectData))]
    [XmlInclude(typeof(ItemFlagsAffectData))]
    [XmlInclude(typeof(ItemWeaponFlagsAffectData))]
    [XmlInclude(typeof(CharacterSizeAffectData))]
    [XmlInclude(typeof(PlagueSpreadAndDamageAffectData))]
    [XmlInclude(typeof(PoisonDamageAffectData))]
    public abstract class AffectDataBase
    {
    }
}
