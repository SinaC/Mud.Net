using System;

namespace Mud.Server
{
    public interface ITimeHandler
    {
        DateTime CurrentTime { get; } // Centralized time synchronized on server's pulse
    }
}
