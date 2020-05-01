using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(typeof(ItemContainerData), typeof(ItemCorpseData), typeof(ItemWeaponData))]
    public class ItemData
    {
        [BsonId]
        public int ItemId { get; set; }

        public int DecayPulseLeft { get; set; }

        public int ItemFlags { get; set; }

        public AuraData[] Auras { get; set; }
    }
}
