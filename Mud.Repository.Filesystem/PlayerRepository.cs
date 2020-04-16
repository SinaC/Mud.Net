using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem
{
    public class PlayerRepository : RepositoryBase, IPlayerRepository
    {
        private string PlayerRepositoryPath => Settings.PlayerRepositoryPath;

        private string BuildFilename(string playerName) => Path.Combine(PlayerRepositoryPath, playerName + ".data");

        public Domain.PlayerData Load(string playerName)
        {
            string filename = BuildFilename(playerName);
            if (!File.Exists(filename))
                return null;

            DataContracts.PlayerData playerData = InnerLoad(filename);
            var mapped = Mapper.Map<DataContracts.PlayerData, Domain.PlayerData>(playerData);

            return mapped;
        }

        public void Save(Domain.PlayerData playerData)
        {
            var mapped = Mapper.Map<Domain.PlayerData, DataContracts.PlayerData>(playerData);

            XmlSerializer serializer = new XmlSerializer(typeof(DataContracts.PlayerData));
            Directory.CreateDirectory(PlayerRepositoryPath);
            string filename = BuildFilename(playerData.Name);
            using (FileStream file = File.Create(filename))
            {
                serializer.Serialize(file, mapped);
            }
        }

        public void Delete(string playerName)
        {
            string filename = BuildFilename(playerName);

            if (File.Exists(filename))
                File.Delete(filename);
        }

        public IEnumerable<string> GetAvatarNames()
        {
            List<string> avatarNames = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(PlayerRepositoryPath))
            {
                DataContracts.PlayerData playerData = InnerLoad(filename);
                if (playerData.Characters.Any())
                    avatarNames.AddRange(playerData.Characters.Select(x => x.Name));
            }
            return avatarNames.ToArray();
        }

        private DataContracts.PlayerData InnerLoad(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(DataContracts.PlayerData));
            using (FileStream file = File.OpenRead(filename))
            {
                return (DataContracts.PlayerData)deserializer.Deserialize(file);
            }
        }
    }
}
