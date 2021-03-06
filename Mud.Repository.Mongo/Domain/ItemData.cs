﻿using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(
        typeof(ItemContainerData), 
        typeof(ItemCorpseData), 
        typeof(ItemWeaponData), 
        typeof(ItemDrinkContainerData), 
        typeof(ItemFoodData), 
        typeof(ItemPortalData),
        typeof(ItemStaffData),
        typeof(ItemWandData))]
    public class ItemData
    {
        [BsonId]
        public int ItemId { get; set; }

        public int Level { get; set; }

        public int DecayPulseLeft { get; set; }

        public string ItemFlags { get; set; }

        public AuraData[] Auras { get; set; }
    }
}
