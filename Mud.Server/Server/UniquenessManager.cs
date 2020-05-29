using Mud.Container;
using Mud.Logger;
using Mud.Repository;
using System.Collections.Generic;

namespace Mud.Server.Server
{
    public class UniquenessManager : IUniquenessManager
    {
        private readonly HashSet<string> _unavailableNames = new HashSet<string>(1024); // Lock ?

        protected IPlayerRepository PlayerRepository { get; }
        protected IAdminRepository AdminRepository { get; }
        protected ILoginRepository LoginRepository { get; }

        public UniquenessManager(IPlayerRepository playerRepository, IAdminRepository adminRepository, ILoginRepository loginRepository)
        {
            PlayerRepository = playerRepository;
            AdminRepository = adminRepository;
            LoginRepository = loginRepository;
        }

        #region IUniquenessManager

        public void Initialize()
        {
            BuildUnavailableNameCacheIfNeeded();
        }

        public bool IsAvatarNameAvailable(string avatarName)
        {
            BuildUnavailableNameCacheIfNeeded();

            return !_unavailableNames.Contains(avatarName);
        }

        public bool IsAccountNameAvailable(string accountName)
        {
            BuildUnavailableNameCacheIfNeeded();

            return !_unavailableNames.Contains(accountName);
        }

        public void AddAvatarName(string avatarName)
        {
            BuildUnavailableNameCacheIfNeeded();

            _unavailableNames.Add(avatarName);
        }
        
        public void RemoveAvatarName(string avatarName)
        {
            BuildUnavailableNameCacheIfNeeded();

            _unavailableNames.Remove(avatarName);
        }
        
        public void RemoveAvatarNames(IEnumerable<string> avatarNames)
        {
            BuildUnavailableNameCacheIfNeeded();

            if (avatarNames != null)
            {
                foreach (var avatarName in avatarNames)
                    _unavailableNames.Remove(avatarName);
            }
        }

        //public void AddAccountName(string accountName) 
        //{
        //}

        //public void RemoveAccountName(string accountName) 
        //{
        //}

        #endregion

        private void BuildUnavailableNameCacheIfNeeded() 
        {
            if (_unavailableNames.Count == 0)
            {
                var logins = LoginRepository.GetLogins();
                foreach (var login in logins)
                    _unavailableNames.Add(login);
                var avatarsFromPlayers = PlayerRepository.GetAvatarNames();
                foreach (var avatarName in avatarsFromPlayers)
                    _unavailableNames.Add(avatarName);
                var avatarsFromAdmins = AdminRepository.GetAvatarNames();
                foreach (var avatarName in avatarsFromAdmins)
                    _unavailableNames.Add(avatarName);
                Log.Default.WriteLine(LogLevels.Info, "UniquenessManager: Unavailable name cache initialized with {0} entries", _unavailableNames.Count);
            }
        }
    }
}
