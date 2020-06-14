using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Mud.Repository.Mongo.Domain;
using Mud.Repository.Mongo.Common;
using AutoMapper;
using Mud.Settings;

namespace Mud.Repository.Mongo
{
    //http://mongodb.github.io/mongo-csharp-driver/2.0/getting_started/quick_tour/
    //http://stackoverflow.com/questions/26573668/set-mongodb-to-serialize-a-class-as-bsondocument
    public class LoginRepository : RepositoryBase<LoginData>, ILoginRepository
    {
        public LoginRepository(IMapper mapper, ISettings settings) 
            : base(mapper, settings, "Login")
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
            Insert(loginData);
            return true;
        }

        public bool CheckUsername(string username, out bool isAdmin)
        {
            LoginData loginData = Collection.AsQueryable().FirstOrDefault(x => x.Username == username);
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
            LoginData loginData = Collection.AsQueryable().FirstOrDefault(x => x.Username == username);
            return loginData != null && loginData.Password == password;
        }

        public bool ChangePassword(string username, string password)
        {
            var update = Builders<LoginData>.Update.Set(x => x.Password, password);
            UpdateResult result = Collection.UpdateOne(x => x.Username == username, update);
            return result.ModifiedCount > 0;
        }

        public bool DeleteLogin(string username)
        {
            DeleteResult deleteResult = Collection.DeleteOne(x => x.Username == username);
            return deleteResult.DeletedCount > 0;
        }

        public bool ChangeAdminStatus(string username, bool isAdmin)
        {
            var update = Builders<LoginData>.Update.Set(x => x.IsAdmin, isAdmin);
            UpdateResult result = Collection.UpdateOne(x => x.Username == username, update);
            return result.ModifiedCount > 0;
        }

        public IEnumerable<string> GetLogins()
        {
            return Collection.AsQueryable().Select(x => x.Username).ToArray();
        }

        #endregion
    }
}
