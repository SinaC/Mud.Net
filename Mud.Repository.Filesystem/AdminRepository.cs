using System.Configuration;
using System.IO;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem
{
    public class AdminRepository : RepositoryBase, IAdminRepository
    {
        private string AdminRepositoryPath => ConfigurationManager.AppSettings["AdminRepositoryPath"];

        private string BuildFilename(string adminName) => Path.Combine(AdminRepositoryPath, adminName + ".data");

        public Domain.AdminData Load(string adminName)
        {
            string filename = BuildFilename(adminName);
            if (!File.Exists(filename))
                return null;

            DataContracts.AdminData adminData;
            XmlSerializer deserializer = new XmlSerializer(typeof(DataContracts.AdminData));
            using (FileStream file = File.OpenRead(filename))
            {
                adminData = (DataContracts.AdminData)deserializer.Deserialize(file);
            }

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
    }
}
