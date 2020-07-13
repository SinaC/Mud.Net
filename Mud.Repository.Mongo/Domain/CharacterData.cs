using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(typeof(PlayableCharacterData), typeof(PetData))]
    public abstract class CharacterData
    {
        public string Name { get; set; }

        public string Race { get; set; }

        public string Class { get; set; }

        public int Level { get; set; }

        public int Sex { get; set; }

        public int Size { get; set; }

        public int HitPoints { get; set; }

        public int MovePoints { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, int> CurrentResources { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, int> MaxResources { get; set; }

        public EquippedItemData[] Equipments { get; set; }

        public ItemData[] Inventory { get; set; }

        public AuraData[] Auras { get; set; }

        public string CharacterFlags { get; set; }

        public string Immunities { get; set; }

        public string Resistances { get; set; }

        public string Vulnerabilities { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, int> Attributes { get; set; } // TODO: this could create duplicate key exception while deserializing if CharacterAttribute is not found anymore
    }
}
