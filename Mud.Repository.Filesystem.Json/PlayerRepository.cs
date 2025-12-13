using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Repository.Filesystem.Json.Resolvers;
using Mud.Repository.Interfaces;
using Mud.Server.Flags.Interfaces;
using System.Text.Json;

namespace Mud.Repository.Filesystem.Json;

[Export(typeof(IPlayerRepository)), Shared]
public class PlayerRepository : IPlayerRepository
{
    private ILogger<PlayerRepository> Logger { get; }
    private string PlayerRepositoryPath { get; }
    private JsonSerializerOptions SerializerOptions { get; }

    private string BuildFilename(string playerName) => Path.Combine(PlayerRepositoryPath, playerName + ".json");

    public PlayerRepository(ILogger<PlayerRepository> logger, IOptions<FileRepositoryOptions> options, IAssemblyHelper assemblyHelper, IFlagFactory flagFactory)
    {
        Logger = logger;
        PlayerRepositoryPath = options.Value.PlayerPath;

        SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            TypeInfoResolver = new PolymorphicTypeResolver(assemblyHelper)
        };
    }

    #region IPlayerRepository

    public PlayerData? Load(string playerName)
    {
        CreateDirectoryIfNeeded();
        string filename = BuildFilename(playerName);
        if (!File.Exists(filename))
            return null;

        using FileStream openStream = File.OpenRead(filename);
        var playerData = JsonSerializer.Deserialize<PlayerData>(openStream, SerializerOptions);

        return playerData;
    }

    public void Save(PlayerData playerData)
    {
        CreateDirectoryIfNeeded();
       
        string filename = BuildFilename(playerData.Name);

        using FileStream createStream = File.Create(filename);
        JsonSerializer.Serialize(createStream, playerData, SerializerOptions);
    }

    public void Delete(string playerName)
    {
        CreateDirectoryIfNeeded();
        string filename = BuildFilename(playerName);

        if (File.Exists(filename))
            File.Delete(filename);
    }

    public IEnumerable<string> GetAvatarNames()
    {
        CreateDirectoryIfNeeded();
        List<string> avatarNames = [];
        foreach (string filename in Directory.EnumerateFiles(PlayerRepositoryPath, "*.json"))
        {
            var playerData = Load(filename);
            if (playerData != null && playerData.Characters.Length != 0)
                avatarNames.AddRange(playerData.Characters.Select(x => x.Name));
        }
        return avatarNames.ToArray();
    }

    #endregion

    private void CreateDirectoryIfNeeded()
    {
        var directory = Path.GetDirectoryName(PlayerRepositoryPath);
        if (directory != null)
            Directory.CreateDirectory(directory);
        else
            Logger.LogError("Invalid directory in player path: {path}", PlayerRepositoryPath);
    }
}
