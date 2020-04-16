using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Mud.Repository.Mongo.Domain;
using Mud.Repository.Mongo.Common;

namespace Mud.Repository.Mongo
{
    //http://mongodb.github.io/mongo-csharp-driver/2.0/getting_started/quick_tour/
    //http://stackoverflow.com/questions/26573668/set-mongodb-to-serialize-a-class-as-bsondocument
    public class LoginRepository : RepositoryBase<LoginData>, ILoginRepository
    {
        public LoginRepository() : base("Login")
        {
        }

        #region ILoginManager

        public bool InsertLogin(string username, string password)
        {
            LoginData loginData = new LoginData
            {
                Username = username,
                Password = password,
                IsAdmin = false,
            };
            MongoRepository.Insert(loginData);
            return true;
        }

        public bool CheckUsername(string username, out bool isAdmin)
        {
            LoginData loginData = MongoRepository.Collection.AsQueryable().FirstOrDefault(x => x.Username == username);
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
            LoginData loginData = MongoRepository.Collection.AsQueryable().FirstOrDefault(x => x.Username == username);
            return loginData != null && loginData.Password == password;
        }

        public bool ChangePassword(string username, string password)
        {
            // TODO
            return true;
        }

        public bool DeleteLogin(string username)
        {
            DeleteResult deleteResult = MongoRepository.Collection.DeleteOne(x => x.Username == username);
            return deleteResult.DeletedCount > 0;
        }

        public bool ChangeAdminStatus(string username, bool isAdmin)
        {
            LoginData loginData = MongoRepository.Collection.AsQueryable().FirstOrDefault(x => x.Username == username);
            if (loginData == null)
                return false;
            loginData.IsAdmin = isAdmin;
            MongoRepository.Collection.ReplaceOne(x => x.Username == username, loginData, new ReplaceOptions { IsUpsert = false });
            return true;
        }

        public IEnumerable<string> GetLogins()
        {
            return MongoRepository.Collection.AsQueryable().Select(x => x.Username).ToArray();
        }

        #endregion

        private void ResetUsernames()
        {
            MongoRepository.Fetch(new List<LoginData>
            { 
                new LoginData
                {
                    Username = "sinac",
                    Password = "password",
                    IsAdmin = true
                },
                new LoginData
                {
                    Username = "player",
                    Password = "password",
                    IsAdmin = false
                }
            });
        }
    }
}
