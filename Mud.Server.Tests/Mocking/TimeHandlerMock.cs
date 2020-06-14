using System;

namespace Mud.Server.Tests.Mocking
{
    internal class TimeHandlerMock : ITimeHandler
    {
        public DateTime CurrentTime => DateTime.Now;
    }
}
