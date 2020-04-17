using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Repository.Tests.Mock
{
    public class MockSettings : ISettings
    {
        public string LogPath => throw new NotImplementedException();

        public string ConnectionString => throw new NotImplementedException();

        public string PlayerRepositoryPath => throw new NotImplementedException();

        public string AdminRepositoryPath => throw new NotImplementedException();

        public string LoginRepositoryFilename => throw new NotImplementedException();

        public string ImportAreaPath => throw new NotImplementedException();

        public bool PrefixForwardedMessages => throw new NotImplementedException();

        public bool ForwardSlaveMessages => throw new NotImplementedException();

        public bool RemovePeriodicAurasInNotInSameRoom => throw new NotImplementedException();

        public int CorpseBlueprintId => throw new NotImplementedException();

        public int PulsePerSeconds => throw new NotImplementedException();

        public int PulsePerMinutes => throw new NotImplementedException();

        public int PulseDelay => throw new NotImplementedException();

        public int PulseViolence => throw new NotImplementedException();

        public bool CheckLoginPassword => throw new NotImplementedException();

        public int IdleMinutesBeforeUnimpersonate => throw new NotImplementedException();

        public int IdleMinutesBeforeDisconnect => throw new NotImplementedException();

        public int MaxLevel => throw new NotImplementedException();

        public int MaxAvatarCount => throw new NotImplementedException();
    }
}
