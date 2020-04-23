using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(typeof(EquipedItemData))]
    public class ItemData
    {
        [BsonId]
        public int ItemId { get; set; }

        public ItemData[] Contains { get; set; }
    }
}
