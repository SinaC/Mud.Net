using System;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using Mud.Datas.DataContracts;

namespace Mud.Datas.Filesystem
{
    public class AdminManager : IAdminManager
    {
        private string AdminRepositoryPath => ConfigurationManager.AppSettings["AdminRepositoryPath"];

        public AdminData Load(string adminName)
        {
            string filename = Path.Combine(AdminRepositoryPath, adminName + ".data");
            if (!File.Exists(filename))
                return null;
            AdminData adminData;
            XmlSerializer deserializer = new XmlSerializer(typeof(AdminData));
            using (FileStream file = File.OpenRead(filename))
            {
                adminData = (AdminData)deserializer.Deserialize(file);
            }
            return adminData;
        }

        public void Save(AdminData data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AdminData));
            Directory.CreateDirectory(AdminRepositoryPath);
            string filename = Path.Combine(AdminRepositoryPath, data.Name + ".data");
            using (FileStream file = File.Create(filename))
            {
                serializer.Serialize(file, data);
            }
        }
    }
}
