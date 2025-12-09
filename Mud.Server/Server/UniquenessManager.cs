using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Repository.Interfaces;
using Mud.Server.Interfaces;

namespace Mud.Server.Server;

[Export(typeof(IUniquenessManager)), Shared]
public class UniquenessManager : IUniquenessManager
{
    private readonly HashSet<string> _unavailableNames = new(1024); // Lock ?

    private ILogger<UniquenessManager> Logger { get; }
    private IPlayerRepository PlayerRepository { get; }
    private IAdminRepository AdminRepository { get; }
    private ILoginRepository LoginRepository { get; }

    public UniquenessManager(ILogger<UniquenessManager> logger, IPlayerRepository playerRepository, IAdminRepository adminRepository, ILoginRepository loginRepository)
    {
        Logger = logger;
        PlayerRepository = playerRepository;
        AdminRepository = adminRepository;
        LoginRepository = loginRepository;
    }

    #region IUniquenessManager

    public void Initialize()
    {
        BuildUnavailableNameCacheIfNeeded();
    }

    public IEnumerable<string> UnavailableNames => _unavailableNames;

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
    
    public void RemoveAvatarNames(IEnumerable<string>? avatarNames)
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
            Logger.LogInformation("UniquenessManager: Unavailable name cache initialized with {count} entries", _unavailableNames.Count);
        }
    }
}
