using System;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using Mud.Datas.DataContracts;

namespace Mud.Datas.Filesystem
{
    public class PlayerManager : IPlayerManager
    {
        #region Singleton

        private static readonly Lazy<PlayerManager> Lazy = new Lazy<PlayerManager>(() => new PlayerManager());

        public static IPlayerManager Instance => Lazy.Value;

        #endregion

        private string PlayerRepositoryPath => ConfigurationManager.AppSettings["PlayerRepositoryPath"];

        private Func<string,string> BuildFilename => (playerName) => Path.Combine(PlayerRepositoryPath, playerName + ".data");

        public PlayerData Load(string playerName)
        {
            string filename = BuildFilename(playerName);

            if (!File.Exists(filename))
                return null;
            PlayerData playerData;
            XmlSerializer deserializer = new XmlSerializer(typeof(PlayerData));
            using (FileStream file = File.OpenRead(filename))
            {
                playerData = (PlayerData) deserializer.Deserialize(file);
            }
            return playerData;
        }

        public void Save(PlayerData data)
        {
            string filename = BuildFilename(data.Name);

            XmlSerializer serializer = new XmlSerializer(typeof(PlayerData));
            Directory.CreateDirectory(PlayerRepositoryPath);
            using (FileStream file = File.Create(filename))
            {
                serializer.Serialize(file, data);
            }
        }

        public void Delete(string playerName)
        {
            string filename = BuildFilename(playerName);

            if (File.Exists(filename))
                File.Delete(filename);
        }
    }
}
