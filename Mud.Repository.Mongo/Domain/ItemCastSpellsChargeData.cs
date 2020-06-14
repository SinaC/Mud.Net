using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(
        typeof(ItemStaffData),
        typeof(ItemWandData))]
    public abstract class ItemCastSpellsChargeData : ItemData
    {
        public int MaxChargeCount { get; set; }
        public int CurrentChargeCount { get; set; }
        public bool AlreadyRecharged { get; set; }
    }
}
