using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mud.Logger;
using Mud.Repository.Filesystem.Common;

namespace Mud.Repository.Filesystem
{
    public class AdminRepository : RepositoryBase, IAdminRepository
    {
        private string AdminRepositoryPath => Settings.AdminRepositoryPath;

        private string BuildFilename(string adminName) => Path.Combine(AdminRepositoryPath, adminName + ".data");

        #region IAdminRepository

        public Mud.Domain.AdminData Load(string adminName)
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
            List<string> avatarNames = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(AdminRepositoryPath))
            {
                Domain.AdminData adminData = Load<Domain.AdminData>(filename);
                if (adminData.Characters.Any())
                    avatarNames.AddRange(adminData.Characters.Select(x => x.Name));
            }
            return avatarNames.ToArray();
        }

        #endregion

        private void CreateDirectoryIfNeeded()
        {
            string directory = Path.GetDirectoryName(AdminRepositoryPath);
            if (directory != null)
                Directory.CreateDirectory(directory);
            else
                Log.Default.WriteLine(LogLevels.Error, "Invalid directory in admin path: {0}", AdminRepositoryPath);
        }
    }
}
