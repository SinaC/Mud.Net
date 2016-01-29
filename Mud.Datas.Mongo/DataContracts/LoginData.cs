using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mud.Datas.Mongo.DataContracts
{
    public class LoginData : Datas.DataContracts.LoginData
    {
        [BsonId]
        public ObjectId _id { get; set; }
    }
}
