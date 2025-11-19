using AutoMapper;
using Microsoft.Extensions.Logging;
using Mud.Repository.Filesystem.Common;
using Mud.Settings.Interfaces;

namespace Mud.Repository.Filesystem;

public class PlayerRepository : RepositoryBase, IPlayerRepository
{
    private string PlayerRepositoryPath => Settings.PlayerRepositoryPath;

    private string BuildFilename(string playerName) => Path.Combine(PlayerRepositoryPath, playerName + ".data");

    public PlayerRepository(ILogger<LoginRepository> logger, IMapper mapper, ISettings settings)
        : base(logger, mapper, settings)
    {
    }

    #region IPlayerRepository

    public Mud.Domain.PlayerData? Load(string playerName)
    {
        CreateDirectoryIfNeeded();
        string filename = BuildFilename(playerName);
        if (!File.Exists(filename))
            return null;

        Domain.PlayerData playerData = Load<Domain.PlayerData>(filename);
        var mapped = Mapper.Map<Domain.PlayerData, Mud.Domain.PlayerData>(playerData);

        return mapped;
    }

    public void Save(Mud.Domain.PlayerData playerData)
    {
        CreateDirectoryIfNeeded();
        var mapped = Mapper.Map<Mud.Domain.PlayerData, Domain.PlayerData>(playerData);

        string filename = BuildFilename(playerData.Name);
        Save(mapped, filename);
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
        foreach (string filename in Directory.EnumerateFiles(PlayerRepositoryPath))
        {
            Domain.PlayerData playerData = Load<Domain.PlayerData>(filename);
            if (playerData.Characters.Length != 0)
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
