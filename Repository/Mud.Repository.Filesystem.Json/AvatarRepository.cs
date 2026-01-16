using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;
using Mud.Repository.Filesystem.Json.Resolvers;
using Mud.Repository.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mud.Repository.Filesystem.Json;

[Export(typeof(IAvatarRepository)), Shared]
public class AvatarRepository : IAvatarRepository
{
    private ILogger<AvatarRepository> Logger { get; }
    private string AvatarRepositoryPath { get; }
    private JsonSerializerOptions SerializerOptions { get; }

    private string BuildFilename(string avatarName) => Path.Combine(AvatarRepositoryPath, avatarName + ".json");

    public AvatarRepository(ILogger<AvatarRepository> logger, IOptions<FileRepositoryOptions> options, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        AvatarRepositoryPath = options.Value.AvatarPath;

        SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            TypeInfoResolver = new PolymorphicTypeResolver(assemblyHelper)
        };
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    #region IAvatarRepository

    public AvatarData? Load(string playerName)
    {
        CreateDirectoryIfNeeded();
        var filename = BuildFilename(playerName);
        if (!File.Exists(filename))
            return null;

        using FileStream openStream = File.OpenRead(filename);
        var playerData = JsonSerializer.Deserialize<AvatarData>(openStream, SerializerOptions);

        return playerData;
    }

    public void Save(AvatarData playerData)
    {
        CreateDirectoryIfNeeded();
        var filename = BuildFilename(playerData.Name);

        using FileStream createStream = File.Create(filename);
        JsonSerializer.Serialize(createStream, playerData, SerializerOptions);
    }

    public void Delete(string playerName)
    {
        CreateDirectoryIfNeeded();
        var filename = BuildFilename(playerName);

        if (File.Exists(filename))
            File.Delete(filename);
    }

    public IEnumerable<string> AvatarNames
    {
        get
        {
            foreach (var filename in Directory.EnumerateFiles(AvatarRepositoryPath, "*.json"))
            {
                var avatarName = Path.GetFileName(filename);
                yield return avatarName;
            }
        }
    }

    #endregion

    private void CreateDirectoryIfNeeded()
    {
        var directory = Path.GetDirectoryName(AvatarRepositoryPath);
        if (directory != null)
            Directory.CreateDirectory(directory);
        else
            Logger.LogError("Invalid directory in avatar path: {path}", AvatarRepositoryPath);
    }
}
