using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.Repository.Filesystem.Common;
using Mud.Repository.Interfaces;

namespace Mud.Repository.Filesystem;

[Export(typeof(IAdminRepository)), Shared]
public class AdminRepository : RepositoryBase, IAdminRepository
{
    private string AdminRepositoryPath => Options.AdminPath;

    private string BuildFilename(string adminName) => Path.Combine(AdminRepositoryPath, adminName + ".data");

    public AdminRepository(ILogger<AdminRepository> logger, IMapper mapper, IOptions<FileRepositoryOptions> options)
        : base(logger, mapper, options)
    {
    }

    #region IAdminRepository

    public Mud.Domain.AdminData? Load(string adminName)
    {
        CreateDirectoryIfNeeded();
        string filename = BuildFilename(adminName);
        if (!File.Exists(filename))
            return null;

        Domain.AdminData adminData = Load<Domain.AdminData>(filename);
        var mapped = Mapper.Map<Domain.AdminData, Mud.Domain.AdminData>(adminData);

        return mapped;
    }

    public void Save(Mud.Domain.AdminData adminData)
    {
        CreateDirectoryIfNeeded();
        var mapped = Mapper.Map<Mud.Domain.AdminData, Domain.AdminData>(adminData);

        string filename = BuildFilename(adminData.Name);
        Save(mapped, filename);
    }

    public IEnumerable<string> GetAvatarNames()
    {
        CreateDirectoryIfNeeded();
        List<string> avatarNames = [];
        foreach (string filename in Directory.EnumerateFiles(AdminRepositoryPath))
        {
            Domain.AdminData adminData = Load<Domain.AdminData>(filename);
            if (adminData.Characters.Length != 0)
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
