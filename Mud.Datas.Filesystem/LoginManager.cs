using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Mud.Datas.DataContracts;
using Mud.Datas.Filesystem.DataContracts;
using Mud.Logger;

namespace Mud.Datas.Filesystem
{
    public class LoginManager : ILoginManager
    {
        private const bool CheckLoginPassword = false;

        private string LoginRepositoryFilename => ConfigurationManager.AppSettings["LoginRepositoryFilename"];

        private Dictionary<string, LoginData> _table = new Dictionary<string, LoginData>();
        private bool _loaded;

        public LoginManager()
        {
            _loaded = false;
        }

        #region ILoginRepository

        public bool InsertLogin(string username, string password)
        {
            if (_table.ContainsKey(username))
                return false;
            _table.Add(username, new LoginData
            {
                Username = username,
                Password = password,
                IsAdmin = false
            });
            Save();
            return true;
        }

        public bool CheckUsername(string username, out bool isAdmin)
        {
            if (!_loaded)
                Load();
            isAdmin = false;
            LoginData loginData;
            bool found = _table.TryGetValue(username, out loginData);
            if (found)
                isAdmin = loginData.IsAdmin;
            return found;
        }

        public bool CheckPassword(string username, string password)
        {
            // TODO: check password + encryption
            if (CheckLoginPassword)
            {
                LoginData loginData;
                _table.TryGetValue(username, out loginData);
                return loginData != null && password == loginData.Password;
            }
            else
                return password != "test";

        }

        public bool ChangePassword(string username, string password)
        {
            LoginData loginData;
            bool found = _table.TryGetValue(username, out loginData);
            if (found)
            {
                loginData.Password = password;
                Save();
                return true;
            }
            return false;
        }

        public bool DeleteLogin(string username)
        {
            if (!_table.ContainsKey(username))
                return false;
            _table.Remove(username);
            Save();
            return true;
        }

        #endregion

        private bool Load()
        {
            // TODO: load logins and check if there is player file not found in logins
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(LoginRepositoryData));
                string directory = Path.GetDirectoryName(LoginRepositoryFilename);
                if (directory != null)
                    Directory.CreateDirectory(directory);
                else
                    Log.Default.WriteLine(LogLevels.Error, "Invalid directory in logins path: {0}", LoginRepositoryFilename);
                using (FileStream file = File.OpenRead(LoginRepositoryFilename))
                {
                    LoginRepositoryData repository = (LoginRepositoryData)deserializer.Deserialize(file);
                    _table = repository.Logins.ToDictionary(x => x.Username);
                }
                _loaded = true;
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "SaveLogins: unable to save. Exception: {0}", ex);
            }
            return true;
        }

        private bool Save()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LoginRepositoryData));
                string directory = Path.GetDirectoryName(LoginRepositoryFilename);
                if (directory != null)
                    Directory.CreateDirectory(directory);
                else
                    Log.Default.WriteLine(LogLevels.Error, "Invalid directory in logins path: {0}", LoginRepositoryFilename);
                using (FileStream file = File.Create(LoginRepositoryFilename))
                {
                    LoginRepositoryData repository = new LoginRepositoryData
                    {
                        Logins = _table.Values.ToList()
                    };
                    serializer.Serialize(file, repository);
                }
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "SaveLogins: unable to save. Exception: {0}", ex);
            }
            return true;
        }
    }
}
