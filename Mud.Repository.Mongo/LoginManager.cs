using System.Linq;
using MongoDB.Driver;
using Mud.Repository.Mongo.Domain;

namespace Mud.Repository.Mongo
{
    //http://mongodb.github.io/mongo-csharp-driver/2.0/getting_started/quick_tour/
    //http://stackoverflow.com/questions/26573668/set-mongodb-to-serialize-a-class-as-bsondocument
    public class LoginManager : ILoginRepository
    {
        #region ILoginManager

        public bool InsertLogin(string username, string password)
        {
            IMongoCollection<LoginData> collection = LoginCollection;
            LoginData loginData = new LoginData
            {
                Username = username,
                Password = password,
                IsAdmin = false,
            };
            collection.InsertOne(loginData);
            return true;
        }

        public bool CheckUsername(string username, out bool isAdmin)
        {
            IMongoCollection<LoginData> collection = LoginCollection;
            LoginData loginData = collection.AsQueryable().FirstOrDefault(x => x.Username == username);
            if (loginData != null)
            {
                isAdmin = loginData.IsAdmin;
                return true;
            }
            isAdmin = false;
            return false;
        }

        public bool CheckPassword(string username, string password)
        {
            IMongoCollection<LoginData> collection = LoginCollection;
            LoginData loginData = collection.AsQueryable().FirstOrDefault(x => x.Username == username);
            return loginData != null && loginData.Password == password;
        }

        public bool ChangePassword(string username, string password)
        {
            // TODO
            return true;
        }

        public bool DeleteLogin(string username)
        {
            IMongoCollection<LoginData> collection = LoginCollection;
            DeleteResult deleteResult = collection.DeleteOne(x => x.Username == username);
            return deleteResult.DeletedCount > 0;
        }

        #endregion

        private IMongoCollection<LoginData> LoginCollection
        {
            get
            {
                MongoClient client = new MongoClient();
                IMongoDatabase db = client.GetDatabase("Mud");
                return db.GetCollection<LoginData>("TestLogins");
            }
        }

        private void ResetUsernames()
        {
            IMongoCollection<LoginData> collection = LoginCollection;
            collection.DeleteMany(x => true);
            collection.InsertOne(new LoginData
            {
                Username = "sinac",
                Password = "password",
                IsAdmin = true
            });
            collection.InsertOne(new LoginData
            {
                Username = "player",
                Password = "password",
                IsAdmin = false
            });
        }
    }
}
