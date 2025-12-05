using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Repository.Filesystem.Json.Converters;
using Mud.Repository.Interfaces;
using Mud.Server.Flags.Interfaces;
using System.Text.Json;

namespace Mud.Repository.Filesystem.Json;

[Export(typeof(IAdminRepository)), Shared]
public class AdminRepository : IAdminRepository
{
    private ILogger<AdminRepository> Logger { get; }
    private string AdminRepositoryPath { get; }
    private JsonSerializerOptions SerializerOptions { get; }

    private string BuildFilename(string adminName) => Path.Combine(AdminRepositoryPath, adminName + ".json");

    public AdminRepository(ILogger<AdminRepository> logger, IOptions<FileRepositoryOptions> options, IFlagFactory flagFactory)
    {
        Logger = logger;
        AdminRepositoryPath = options.Value.AdminPath;

        SerializerOptions = new JsonSerializerOptions { WriteIndented = true };
        SerializerOptions.Converters.Add(new CharacterFlagsJsonConverter(flagFactory));
        SerializerOptions.Converters.Add(new IRVFlagsJsonConverter(flagFactory));
        SerializerOptions.Converters.Add(new ShieldFlagsJsonConverter(flagFactory));
        SerializerOptions.Converters.Add(new ItemFlagsJsonConverter(flagFactory));
        SerializerOptions.Converters.Add(new WeaponFlagsJsonConverter(flagFactory));
        SerializerOptions.Converters.Add(new RoomFlagsJsonConverter(flagFactory));
    }

    #region IAdminRepository

    public AdminData? Load(string adminName)
    {
        CreateDirectoryIfNeeded();
        var filename = BuildFilename(adminName);
        if (!File.Exists(filename))
            return null;

        using FileStream openStream = File.OpenRead(filename);
        var adminData = JsonSerializer.Deserialize<AdminData>(openStream, SerializerOptions);

        return adminData;
    }

    public void Save(AdminData adminData)
    {
        CreateDirectoryIfNeeded();
        var filename = BuildFilename(adminData.Name);

        using FileStream createStream = File.Create(filename);
        JsonSerializer.Serialize(createStream, adminData, SerializerOptions);
    }

    public IEnumerable<string> GetAvatarNames()
    {
        CreateDirectoryIfNeeded();
        List<string> avatarNames = [];
        foreach (string filename in Directory.EnumerateFiles(AdminRepositoryPath, "*.json"))
        {
            var adminData = Load(filename);
            if (adminData != null && adminData.Characters.Length != 0)
                avatarNames.AddRange(adminData.Characters.Select(x => x.Name));
        }
        return avatarNames.ToArray();
    }

    #endregion

    private void CreateDirectoryIfNeeded()
    {
        var directory = Path.GetDirectoryName(AdminRepositoryPath);
        if (directory != null)
            Directory.CreateDirectory(directory);
        else
            Logger.LogError("Invalid directory in admin path: {path}", AdminRepositoryPath);
    }
}
