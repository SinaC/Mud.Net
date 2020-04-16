using System.Configuration;

namespace Mud.Settings
{
    public class Settings : ISettings
    {
        #region ISettings

        public string LogPath => ConfigurationManager.AppSettings["logpath"];

        public string ConnectionString => ConfigurationManager.AppSettings["ConnectionString"];

        public string PlayerRepositoryPath => ConfigurationManager.AppSettings["PlayerRepositoryPath"];

        public string AdminRepositoryPath => ConfigurationManager.AppSettings["AdminRepositoryPath"];

        public string LoginRepositoryFilename => ConfigurationManager.AppSettings["LoginRepositoryFilename"];

        public string ImportAreaPath => ConfigurationManager.AppSettings["ImportAreaPath"];

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
        private bool BoolSetting(string key, bool defaultValue) => SafeConvertToBool(ConfigurationManager.AppSettings[key]) ?? defaultValue;

        private int IntSetting(string key, int defaultValue) => SafeConvertToInt(ConfigurationManager.AppSettings[key]) ?? defaultValue;

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
    }
}
