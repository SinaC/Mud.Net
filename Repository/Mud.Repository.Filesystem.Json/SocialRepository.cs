using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Repository.Interfaces;
using System.Text.Json;

namespace Mud.Repository.Filesystem.Json;

[Export(typeof(ISocialRepository)), Shared]
public class SocialRepository(ILogger<AccountRepository> logger, IOptions<FileRepositoryOptions> options) : ISocialRepository
{
    private string SocialRepositoryPath { get; } = options.Value.SocialPath;

    #region ISocialRepository

    public IReadOnlyCollection<SocialData> Load()
    {
        CreateDirectoryIfNeeded();

        var socials = new List<SocialData>();
        foreach (var filename in Directory.EnumerateFiles(SocialRepositoryPath, "*.json"))
        {
            logger.LogInformation("Loading social data from file: {file}", filename);

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
            logger.LogError("Invalid directory in account path: {path}", SocialRepositoryPath);
    }
}
