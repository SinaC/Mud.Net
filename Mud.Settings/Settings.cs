using System;
using System.Configuration;
using System.IO;

namespace Mud.Settings
{
    public class Settings : ISettings
    {
        private const string SettingsPathKey = "SettingsPath";

        #region ISettings

        public string LogPath => this["logpath"];

        public int TelnetPort => IntSetting("port", 11000);

        public bool UseMongo => BoolSetting("UseMongo", false);

        public string ConnectionString => this["ConnectionString"];

        public string PlayerRepositoryPath => this["PlayerRepositoryPath"];

        public string AdminRepositoryPath => this["AdminRepositoryPath"];

        public string LoginRepositoryFilename => this["LoginRepositoryFilename"];

        public string ImportAreaPath => this["ImportAreaPath"];

        // Add <IMP> or <CTRL> before forwarding a message
        public bool PrefixForwardedMessages => BoolSetting("PrefixForwardedMessages", false);

        // Forward messages received by a slaved character
        public bool ForwardSlaveMessages => BoolSetting("ForwardSlaveMessages", false);

        // If a NPC has dot/hot from a source in another room, they are removed on next Pulse
        public bool RemovePeriodicAurasInNotInSameRoom => BoolSetting("RemovePeriodicAurasInNotInSameRoom", false);

        //
        public int CorpseBlueprintId => IntSetting("CorpseBlueprintId", 999999);

        // Pulse
        public int PulsePerSeconds => IntSetting("PulsePerSeconds", 4);

        public int PulsePerMinutes => PulsePerSeconds * 60;

        public int PulseDelay => 1000 / PulsePerSeconds;

        // automatic combat (in pulse per seconds)
        public int PulseViolence => 3 * PulsePerSeconds;

        //
        public bool CheckLoginPassword => BoolSetting("CheckLoginPassword", false);

        //
        public int IdleMinutesBeforeUnimpersonate => IntSetting("IdleMinutesBeforeUnimpersonate", 10);

        public int IdleMinutesBeforeDisconnect => IntSetting("IdleMinutesBeforeDisconnect", 20);

        //
        public int MaxLevel => IntSetting("MaxLevel", 60);

        public int MaxAvatarCount => IntSetting("MaxAvatarCount", 10);

        #endregion

        //
        private readonly Lazy<Configuration> _lazyCustomConfig = new Lazy<Configuration>(() => ReadCustomConfig(ConfigurationManager.AppSettings[SettingsPathKey]));

        //
        private string this[string key] => _lazyCustomConfig.Value.AppSettings.Settings[key]?.Value;

        //
        private bool BoolSetting(string key, bool defaultValue) => SafeConvertToBool(this[key]) ?? defaultValue;

        private int IntSetting(string key, int defaultValue) => SafeConvertToInt(this[key]) ?? defaultValue;

        private static bool? SafeConvertToBool(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            bool b;
            if (bool.TryParse(s, out b))
                return b;
            return false;
        }

        private static int? SafeConvertToInt(string s)
        {
            int i;
            if (int.TryParse(s, out i))
                return i;
            return null;
        }

        private static Configuration ReadCustomConfig(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ConfigurationErrorsException($"{SettingsPathKey} not found in app.config");
            if (File.Exists(path))
                return ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
                {
                    ExeConfigFilename = path
                }, ConfigurationUserLevel.None);
            throw new ConfigurationErrorsException($"Settings file '{path}' not doesn't exist");
        }
    }
}
