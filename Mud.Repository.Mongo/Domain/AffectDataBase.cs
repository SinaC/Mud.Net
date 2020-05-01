using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(typeof(CharacterAttributeAffectData), typeof(CharacterFlagsAffectData), typeof(CharacterIRVAffectData), typeof(CharacterSexAffectData), typeof(ItemFlagsAffectData), typeof(ItemWeaponFlagsAffectData))]
    public abstract class AffectDataBase
    {
    }
}
