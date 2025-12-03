using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.Repository.Filesystem.Json.Domain;
using Mud.Repository.Interfaces;
using System.Text.Json;

namespace Mud.Repository.Filesystem.Json;

[Export(typeof(ILoginRepository)), Shared]
public class LoginRepository : ILoginRepository
{
    private const bool CheckLoginPassword = false;

    private ILogger<LoginRepository> Logger { get; }
    private string LoginRepositoryFilename { get; }
    private JsonSerializerOptions SerializerOptions { get; }

    private Dictionary<string, LoginData> _table = [];
    private bool _loaded;

    public LoginRepository(ILogger<LoginRepository> logger, IOptions<FileRepositoryOptions> options)
    {
        Logger = logger;
        LoginRepositoryFilename = options.Value.LoginFilename;
        SerializerOptions = new JsonSerializerOptions { WriteIndented = true };

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
        var found = _table.TryGetValue(username, out var loginData);
        if (found)
            isAdmin = loginData!.IsAdmin;
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

        var found = _table.TryGetValue(username, out var loginData);
        if (found)
        {
            loginData!.Password = password;
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

        var found = _table.TryGetValue(username, out var loginData);
        if (found)
        {
            loginData!.IsAdmin = isAdmin;
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
            var directory = Path.GetDirectoryName(LoginRepositoryFilename);
            if (directory != null)
                Directory.CreateDirectory(directory);
            else
                Logger.LogError("Invalid directory in logins path: {filename}", LoginRepositoryFilename);
            using FileStream openStream = File.OpenRead(LoginRepositoryFilename);
            var adminData = JsonSerializer.Deserialize<LoginRepositoryData>(openStream, SerializerOptions);
            if (adminData != null)
                _table = adminData.Logins.ToDictionary(x => x.Username);
            _loaded = true;
        }
        catch (Exception ex)
        {
            Logger.LogError("LoadIfNeeded: unable to load. Exception: {ex}", ex);
        }
    }

    private void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(LoginRepositoryFilename);
            if (directory != null)
                Directory.CreateDirectory(directory);
            else
                Logger.LogError("Invalid directory in logins path: {filename}", LoginRepositoryFilename);
            using FileStream createStream = File.Create(LoginRepositoryFilename);

            LoginRepositoryData repository = new()
            {
                Logins = _table.Values.ToArray()
            };
            JsonSerializer.Serialize(createStream, repository, SerializerOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError("Save: unable to save. Exception: {ex}", ex);
        }
    }
}
