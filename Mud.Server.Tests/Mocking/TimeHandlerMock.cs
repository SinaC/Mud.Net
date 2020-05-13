using System;
using Mud.Domain;

namespace Mud.Server.Tests.Mocking
{
    internal class TimeHandlerMock : ITimeManager
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
    }
}
