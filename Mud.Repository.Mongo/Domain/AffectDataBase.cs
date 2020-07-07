using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(typeof(CharacterAttributeAffectData), 
        typeof(CharacterFlagsAffectData), 
        typeof(CharacterIRVAffectData), 
        typeof(CharacterSexAffectData), 
        typeof(ItemFlagsAffectData), 
        typeof(ItemWeaponFlagsAffectData),
        typeof(PlagueSpreadAndDamageAffectData),
        typeof(PoisonDamageAffectData),
        typeof(RoomFlagsAffectData),
        typeof(RoomHealRateAffectData),
        typeof(RoomResourceRateAffectData))]
    public abstract class AffectDataBase
    {
    }
}
