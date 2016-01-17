using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Datas.Mongo
{
    public class LoginData
    {
        [BsonId]
        public MongoDB.Bson.ObjectId _id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
    }
}
