using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Mud.Datas.Mongo
{
    public class LoginManager : ILoginManager
    {
        #region ILoginManager

        public bool InsertLogin(string username, string password)
        {
            throw new NotImplementedException();
        }

        public bool CheckUsername(string username, out bool isAdmin)
        {
            //throw new NotImplementedException();
            TestClient();
            isAdmin = true;
            return true;
        }

        public bool CheckPassword(string username, string password)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void TestClient()
        {
            MongoClient client = new MongoClient();
            var list = client.ListDatabases();
        }
    }
}
