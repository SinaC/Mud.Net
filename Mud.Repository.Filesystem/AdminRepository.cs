using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem
{
    public class AdminRepository : RepositoryBase, IAdminRepository
    {
        private string AdminRepositoryPath => Settings.AdminRepositoryPath;

        private string BuildFilename(string adminName) => Path.Combine(AdminRepositoryPath, adminName + ".data");

        public Domain.AdminData Load(string adminName)
        {
            string filename = BuildFilename(adminName);
            if (!File.Exists(filename))
                return null;

            DataContracts.AdminData adminData = InnerLoad(filename);
            var mapped = Mapper.Map<DataContracts.AdminData, Domain.AdminData>(adminData);

            return mapped;
        }

        public void Save(Domain.AdminData adminData)
        {
            var mapped = Mapper.Map<Domain.AdminData, DataContracts.AdminData>(adminData);

            XmlSerializer serializer = new XmlSerializer(typeof(DataContracts.AdminData));
            Directory.CreateDirectory(AdminRepositoryPath);
            string filename = BuildFilename(adminData.Name);
            using (FileStream file = File.Create(filename))
            {
                serializer.Serialize(file, mapped);
            }
        }

        public IEnumerable<string> GetAvatarNames() 
        {
            List<string> avatarNames = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(AdminRepositoryPath))
            {
                DataContracts.AdminData adminData = InnerLoad(filename);
                if (adminData.Characters.Any())
                    avatarNames.AddRange(adminData.Characters.Select(x => x.Name));
            }
            return avatarNames.ToArray();
        }

        private DataContracts.AdminData InnerLoad(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(DataContracts.AdminData));
            using (FileStream file = File.OpenRead(filename))
            {
                return (DataContracts.AdminData)deserializer.Deserialize(file);
            }
        }
    }
}
