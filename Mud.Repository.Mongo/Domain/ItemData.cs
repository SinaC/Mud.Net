using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(typeof(EquipedItemData))]
    public class ItemData
    {
        [BsonId]
        public int ItemId { get; set; }

        public List<ItemData> Contains { get; set; }
    }
}
