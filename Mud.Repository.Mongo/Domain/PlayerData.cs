using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Repository.Mongo.Domain
{
    [BsonKnownTypes(typeof(AdminData))]
    public class PlayerData
    {
        [BsonId]
        public string Name { get; set; }

        public Dictionary<string, string> Aliases { get; set; }

        public List<CharacterData> Characters { get; set; }
    }
}
