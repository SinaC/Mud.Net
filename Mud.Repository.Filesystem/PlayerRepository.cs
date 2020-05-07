using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mud.Logger;
using Mud.Repository.Filesystem.Common;

namespace Mud.Repository.Filesystem
{
    public class PlayerRepository : RepositoryBase, IPlayerRepository
    {
        private string PlayerRepositoryPath => Settings.PlayerRepositoryPath;

        private string BuildFilename(string playerName) => Path.Combine(PlayerRepositoryPath, playerName + ".data");

        #region IPlayerRepository

        public Domain.PlayerData Load(string playerName)
        {
            CreateDirectoryIfNeeded();
            string filename = BuildFilename(playerName);
            if (!File.Exists(filename))
                return null;

            DataContracts.PlayerData playerData = Load<DataContracts.PlayerData>(filename);
            var mapped = Mapper.Map<DataContracts.PlayerData, Domain.PlayerData>(playerData);

            return mapped;
        }

        public void Save(Domain.PlayerData playerData)
        {
            CreateDirectoryIfNeeded();
            var mapped = Mapper.Map<Domain.PlayerData, DataContracts.PlayerData>(playerData);

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
            List<string> avatarNames = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(PlayerRepositoryPath))
            {
                DataContracts.PlayerData playerData = Load<DataContracts.PlayerData>(filename);
                if (playerData.Characters.Any())
                    avatarNames.AddRange(playerData.Characters.Select(x => x.Name));
            }
            return avatarNames.ToArray();
        }

        #endregion

        private void CreateDirectoryIfNeeded()
        {
            string directory = Path.GetDirectoryName(PlayerRepositoryPath);
            if (directory != null)
                Directory.CreateDirectory(directory);
            else
                Log.Default.WriteLine(LogLevels.Error, "Invalid directory in player path: {0}", PlayerRepositoryPath);
        }
    }
}
