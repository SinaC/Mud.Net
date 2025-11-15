using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.Domain;

[XmlInclude(typeof(CharacterAttributeAffectData))]
[XmlInclude(typeof(CharacterFlagsAffectData))]
[XmlInclude(typeof(CharacterIRVAffectData))]
[XmlInclude(typeof(CharacterSexAffectData))]
[XmlInclude(typeof(ItemFlagsAffectData))]
[XmlInclude(typeof(ItemWeaponFlagsAffectData))]
[XmlInclude(typeof(CharacterSizeAffectData))]
[XmlInclude(typeof(PlagueSpreadAndDamageAffectData))]
[XmlInclude(typeof(PoisonDamageAffectData))]
[XmlInclude(typeof(RoomFlagsAffectData))]
[XmlInclude(typeof(RoomHealRateAffectData))]
[XmlInclude(typeof(RoomResourceRateAffectData))]
public abstract class AffectDataBase
{
}
