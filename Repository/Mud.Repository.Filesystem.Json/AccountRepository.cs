using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Account;
using Mud.Repository.Filesystem.Json.Resolvers;
using Mud.Repository.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mud.Repository.Filesystem.Json;

[Export(typeof(IAccountRepository)), Shared]
public class AccountRepository : IAccountRepository
{
    private ILogger<AccountRepository> Logger { get; }
    private string AccountRepositoryPath { get; }
    private JsonSerializerOptions SerializerOptions { get; }

    private string BuildFilename(string accountName) => Path.Combine(AccountRepositoryPath, accountName + ".json");

    public AccountRepository(ILogger<AccountRepository> logger, IOptions<FileRepositoryOptions> options, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        AccountRepositoryPath = options.Value.AccountPath;

        SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            TypeInfoResolver = new PolymorphicTypeResolver(assemblyHelper)
        };
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    #region IAccountRepository

    public AccountData? Load(string accountName)
    {
        CreateDirectoryIfNeeded();
        var filename = BuildFilename(accountName);
        if (!File.Exists(filename))
            return null;

        using FileStream openStream = File.OpenRead(filename);
        var accountData = JsonSerializer.Deserialize<AccountData>(openStream, SerializerOptions);

        return accountData;
    }

    public void Save(AccountData accountData)
    {
        CreateDirectoryIfNeeded();
        var filename = BuildFilename(accountData.Username);

        using FileStream createStream = File.Create(filename);
        JsonSerializer.Serialize(createStream, accountData, SerializerOptions);
    }

    public void Delete(string accountName)
    {
        CreateDirectoryIfNeeded();
        var filename = BuildFilename(accountName);

        if (File.Exists(filename))
            File.Delete(filename);
    }

    public IEnumerable<string> AccountNames
    {
        get
        {
            foreach (var filename in Directory.EnumerateFiles(AccountRepositoryPath, "*.json"))
            {
                var accountName = Path.GetFileName(filename);
                yield return accountName;
            }
        }
    }

    public IEnumerable<string> AvatarNames
    {
        get
        {
            foreach (var filename in Directory.EnumerateFiles(AccountRepositoryPath, "*.json"))
            {
                var accountData = Load(filename);
                if (accountData != null && accountData.AvatarMetaDatas != null && accountData.AvatarMetaDatas.Length != 0)
                {
                    foreach(var avatarMetaData in accountData.AvatarMetaDatas)
                        yield return avatarMetaData.Name;
                }
            }
        }
    }

    #endregion

    private void CreateDirectoryIfNeeded()
    {
        var directory = Path.GetDirectoryName(AccountRepositoryPath);
        if (directory != null)
            Directory.CreateDirectory(directory);
        else
            Logger.LogError("Invalid directory in account path: {path}", AccountRepositoryPath);
    }
}
