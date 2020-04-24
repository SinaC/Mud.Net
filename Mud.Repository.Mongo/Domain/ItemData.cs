using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(typeof(ItemContainerData), typeof(ItemCorpseData))]
    public class ItemData
    {
        [BsonId]
        public int ItemId { get; set; }

        public int DecayPulseLeft { get; set; }
    }
}
