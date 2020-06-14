using Mud.Settings;
using System;

namespace Mud.Server.Tests.Mocking
{
    internal class SettingsMock : ISettings
    {
        public string LogPath => throw new NotImplementedException();

        public int TelnetPort => throw new NotImplementedException();

        public bool UseMongo => throw new NotImplementedException();

        public string ConnectionString => throw new NotImplementedException();

        public string PlayerRepositoryPath => throw new NotImplementedException();

        public string AdminRepositoryPath => throw new NotImplementedException();

        public string LoginRepositoryFilename => throw new NotImplementedException();

        public string ImportAreaPath => throw new NotImplementedException();

        public bool PrefixForwardedMessages => throw new NotImplementedException();

        public bool ForwardSlaveMessages => throw new NotImplementedException();

        public bool RemovePeriodicAurasInNotInSameRoom => throw new NotImplementedException();

        public bool PerformSanityCheck => throw new NotImplementedException();

        public bool DumpOnInitialize => throw new NotImplementedException();

        public int CoinsBlueprintId => throw new NotImplementedException();

        public int CorpseBlueprintId => throw new NotImplementedException();

        public int LightBallBlueprintId => throw new NotImplementedException();

        public int FloatingDiscBlueprintId => throw new NotImplementedException();

        public int DefaultRoomId => throw new NotImplementedException();

        public int DefaultRecallRoomId => throw new NotImplementedException();

        public bool CheckLoginPassword => throw new NotImplementedException();

        public int IdleMinutesBeforeUnimpersonate => throw new NotImplementedException();

        public int IdleMinutesBeforeDisconnect => throw new NotImplementedException();

        public int MaxLevel => throw new NotImplementedException();

        public int MaxAvatarCount => throw new NotImplementedException();

        public int MushroomBlueprintId => throw new NotImplementedException();

        public int SpringBlueprintId => throw new NotImplementedException();

        public int PortalBlueprintId => throw new NotImplementedException();

        public int RoseBlueprintId => throw new NotImplementedException();

        public int NullRoomId => throw new NotImplementedException();

        public int DefaultDeathRoomId => throw new NotImplementedException();

        public int MudSchoolRoomId => throw new NotImplementedException();
    }
}
