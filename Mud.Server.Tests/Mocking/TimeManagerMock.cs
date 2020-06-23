using System;
using Mud.Domain;
using Mud.Server.Interfaces;

namespace Mud.Server.Tests.Mocking
{
    internal class TimeManagerMock : ITimeManager
    {
        public DateTime CurrentTime => DateTime.Now;

        public int Hour => throw new NotImplementedException();
        public int Day => throw new NotImplementedException();
        public int Month => throw new NotImplementedException();
        public int Year => throw new NotImplementedException();

        public int Pressure => throw new NotImplementedException();
        public int PressureChange => throw new NotImplementedException();
        public SunPhases SunPhase => throw new NotImplementedException();
        public SkyStates SkyState => throw new NotImplementedException();

        public int MoonCount => throw new NotImplementedException();

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void FixCurrentTime()
        {
            throw new NotImplementedException();
        }

        public string Update()
        {
            throw new NotImplementedException();
        }

        public void ChangePressure(int changeValue)
        {
            throw new NotImplementedException();
        }

        public int MoonPhase(int moonId)
        {
            throw new NotImplementedException();
        }

        public bool IsMoonNight()
        {
            throw new NotImplementedException();
        }

        public bool IsMoonInSky(int moonId)
        {
            throw new NotImplementedException();
        }

        public bool IsMoonVisible(int moonId)
        {
            throw new NotImplementedException();
        }

        public bool IsMoonFull(int moonId)
        {
            throw new NotImplementedException();
        }

        public string MoonInfo(int moonId)
        {
            throw new NotImplementedException();
        }

        public string TimeInfo()
        {
            throw new NotImplementedException();
        }
    }
}
