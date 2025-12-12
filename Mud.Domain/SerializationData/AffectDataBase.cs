using System.Text.Json.Serialization;

namespace Mud.Domain.SerializationData;

[JsonDerivedType(typeof(CharacterAdditionalHitAffectData), "characterAdditionalHitAffectData")]
[JsonDerivedType(typeof(CharacterAttributeAffectData), "characterAttribute")]
[JsonDerivedType(typeof(CharacterFlagsAffectData), "characterFlags")]
[JsonDerivedType(typeof(CharacterShieldFlagsAffectData), "shieldFlags")]
[JsonDerivedType(typeof(CharacterIRVAffectData), "irv")]
[JsonDerivedType(typeof(CharacterSexAffectData), "sex")]
[JsonDerivedType(typeof(ItemFlagsAffectData), "itemFlags")]
[JsonDerivedType(typeof(ItemWeaponFlagsAffectData), "weaponFlags")]
[JsonDerivedType(typeof(CharacterSizeAffectData), "size")]
[JsonDerivedType(typeof(PlagueSpreadAndDamageAffectData), "plague")]
[JsonDerivedType(typeof(PoisonDamageAffectData), "poison")]
[JsonDerivedType(typeof(RoomFlagsAffectData), "roomFlags")]
[JsonDerivedType(typeof(RoomHealRateAffectData), "roomHealRate")]
[JsonDerivedType(typeof(RoomResourceRateAffectData), "roomSourceRate")]
public abstract class AffectDataBase
{
}
