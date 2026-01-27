using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Repository.Interfaces;
using System.Text.Json;

namespace Mud.Repository.Filesystem.Json;

[Export(typeof(ISocialRepository)), Shared]
public class SocialRepository : ISocialRepository
{
    private ILogger<AccountRepository> Logger { get; }
    private string SocialRepositoryPath { get; }

    public SocialRepository(ILogger<AccountRepository> logger, IOptions<FileRepositoryOptions> options, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        SocialRepositoryPath = options.Value.SocialPath;
    }

    #region ISocialRepository

    public IReadOnlyCollection<SocialData> Load()
    {
        CreateDirectoryIfNeeded();

        var socials = new List<SocialData>();
        foreach (var filename in Directory.EnumerateFiles(SocialRepositoryPath, "*.json"))
        {
            Logger.LogInformation("Loading social data from file: {file}", filename);

            using FileStream openStream = File.OpenRead(filename);
            var socialData = JsonSerializer.Deserialize<SocialData[]>(openStream);
            if (socialData != null)
                socials.AddRange(socialData);
        }

        return socials;
    }

    #endregion

    private void CreateDirectoryIfNeeded()
    {
        var directory = Path.GetDirectoryName(SocialRepositoryPath);
        if (directory != null)
            Directory.CreateDirectory(directory);
        else
            Logger.LogError("Invalid directory in account path: {path}", SocialRepositoryPath);
    }
}
