using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Mud.Repository.Filesystem.Common;
using Mud.Logger;
using Mud.Repository.Filesystem.Domain;
using Mud.Settings;
using AutoMapper;

namespace Mud.Repository.Filesystem
{
    public class LoginRepository : RepositoryBase, ILoginRepository
    {
        private const bool CheckLoginPassword = false;

        private string LoginRepositoryFilename => Settings.LoginRepositoryFilename;

        private Dictionary<string, LoginData> _table = new Dictionary<string, LoginData>();
        private bool _loaded;

        public LoginRepository(IMapper mapper, ISettings settings)
            : base(mapper, settings)
        {
            _loaded = false;
        }

        #region ILoginRepository

        public bool InsertLogin(string username, string password)
        {
            LoadIfNeeded();

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
            LoadIfNeeded();

            isAdmin = false;
            LoginData loginData;
            bool found = _table.TryGetValue(username, out loginData);
            if (found)
                isAdmin = loginData.IsAdmin;
            return found;
        }

        public bool CheckPassword(string username, string password)
        {
            LoadIfNeeded();

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
            LoadIfNeeded();

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
            LoadIfNeeded();

            if (!_table.ContainsKey(username))
                return false;
            _table.Remove(username);
            Save();
            return true;
        }

        public bool ChangeAdminStatus(string username, bool isAdmin)
        {
            LoadIfNeeded();

            LoginData loginData;
            bool found = _table.TryGetValue(username, out loginData);
            if (found)
            {
                loginData.IsAdmin = isAdmin;
                Save();
                return true;
            }
            return false;
        }

        public IEnumerable<string> GetLogins()
        {
            LoadIfNeeded();

            return _table.Keys.ToArray();
        }

        #endregion

        private void LoadIfNeeded()
        {
            if (_loaded)
                return;
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
                Log.Default.WriteLine(LogLevels.Error, "LoadIfNeeded: unable to load. Exception: {0}", ex);
            }
            return;
        }

        private void Save()
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
                        Logins = _table.Values.ToArray()
                    };
                    serializer.Serialize(file, repository);
                }
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Save: unable to save. Exception: {0}", ex);
            }
            return;
        }
    }
}
