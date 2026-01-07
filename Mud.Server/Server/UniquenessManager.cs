using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.Repository.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Options;

namespace Mud.Server.Server;

[Export(typeof(IUniquenessManager)), Shared]
public class UniquenessManager : IUniquenessManager
{
    private readonly HashSet<string> _unavailableNames = new(1024);

    private ILogger<UniquenessManager> Logger { get; }
    public IAccountRepository AccountRepository { get; }
    public IAvatarRepository AvatarRepository { get; }

    public UniquenessManager(ILogger<UniquenessManager> logger, IAccountRepository accountRepository, IAvatarRepository avatarRepository, IOptions<ServerOptions> options)
    {
        Logger = logger;
        AccountRepository = accountRepository;
        AvatarRepository = avatarRepository;

        foreach(var forbiddenName in options.Value.ForbiddenNames)
            _unavailableNames.Add(forbiddenName);
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

    public void AddAccountName(string accountName)
    {
        BuildUnavailableNameCacheIfNeeded();

        _unavailableNames.Add(accountName);
    }

    public void RemoveAccountName(string accountName)
    {
        BuildUnavailableNameCacheIfNeeded();

        _unavailableNames.Remove(accountName);
    }

    #endregion

    private void BuildUnavailableNameCacheIfNeeded() 
    {
        if (_unavailableNames.Count == 0)
        {
            var accountNames = AccountRepository.AccountNames;
            foreach (var accountName in accountNames)
                _unavailableNames.Add(accountName);
            var avatarNames = AvatarRepository.AvatarNames;
            foreach (var avatarName in avatarNames)
                _unavailableNames.Add(avatarName);
            Logger.LogInformation("UniquenessManager: Unavailable name cache initialized with {count} entries", _unavailableNames.Count);
        }
    }
}
