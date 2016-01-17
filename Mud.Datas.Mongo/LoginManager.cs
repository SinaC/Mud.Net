using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Mud.Datas.Mongo
{
    public class LoginManager : ILoginManager
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

        #endregion

        ////http://mongodb.github.io/mongo-csharp-driver/2.0/getting_started/quick_tour/
        ////http://stackoverflow.com/questions/26573668/set-mongodb-to-serialize-a-class-as-bsondocument
        //private void TestClient()
        //{
        //    MongoClient client = new MongoClient();
        //    IMongoDatabase db = client.GetDatabase("Mud");
        //    //db.CreateCollection("Logins");
        //    IMongoCollection<MultipleStringTest> collection = db.GetCollection<MultipleStringTest>("Test");
        //    collection.DeleteMany(test => true);
        //    MultipleStringTest doc1 = new MultipleStringTest
        //    {
        //        Title = "titre1",
        //        Name = "nom1"
        //    };
        //    collection.InsertOne(doc1);
        //    collection.AsQueryable().Where(x => x.Name.StartsWith("nom"));
        //}

        //public class MultipleStringTest
        //{
        //    public string Title { get; set; }
        //    public string Name { get; set; }
        //}

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
