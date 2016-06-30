using System.IO;
using System.Xml.Serialization;
using Mud.Datas.DataContracts;

namespace Mud.Datas.Filesystem
{
    public class PlayerManager : IPlayerManager
    {
        private const string PlayerRepositoryPath = @"D:\Github\Mud.Net\Datas\Players\";

        public PlayerData Load(string playerName)
        {
            PlayerData playerData;
            XmlSerializer deserializer = new XmlSerializer(typeof(PlayerData));
            string filename = Path.Combine(PlayerRepositoryPath, playerName + ".data");
            using (FileStream file = File.OpenRead(filename))
            {
                playerData = (PlayerData) deserializer.Deserialize(file);
            }
            return playerData;
        }

        public void Save(PlayerData data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerData));
            Directory.CreateDirectory(PlayerRepositoryPath);
            string filename = Path.Combine(PlayerRepositoryPath, data.Name + ".data");
            using (FileStream file = File.Create(filename))
            {
                serializer.Serialize(file, data);
            }
        }
    }
}
