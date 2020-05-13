using System;
using Mud.Domain;

namespace Mud.Server
{
    public interface ITimeManager
    {
        // Centralized time synchronized on server's pulse
        DateTime CurrentTime { get; }

        // World' specific time values
        int Hour { get; }
        int Day { get; }
        int Month { get; }
        int Year { get; }

        // Weather
        int Pressure { get; } // in mmHg
        int PressureChange { get; }
        SunPhases SunPhase { get; }
        SkyStates SkyState { get; }

        //
        void Initialize();
        void FixCurrentTime();
        string Update(); // increase hour by one, update time and weather
        void ChangePressure(int changeValue);
    }
}
