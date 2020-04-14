using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Repository.Mongo.Domain
{
    public class LoginData
    {
        [BsonId]
        public string Username { get; set; }

        public string Password { get; set; } // TODO: crypt

        public bool IsAdmin { get; set; }
    }
}
