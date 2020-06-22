using System;
using Mud.Domain;

namespace Mud.Server.Interfaces
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
        string TimeInfo();

        // Weather
        int Pressure { get; } // in mmHg
        int PressureChange { get; }
        SunPhases SunPhase { get; }
        SkyStates SkyState { get; }

        // Moons
        int MoonCount { get; }
        int MoonPhase(int moonId);
        bool IsMoonNight();
        bool IsMoonInSky(int moonId);
        bool IsMoonVisible(int moonId);
        bool IsMoonFull(int moonId);
        string MoonInfo(int moonId);

        //
        void Initialize();
        void FixCurrentTime();
        string Update(); // increase hour by one, update time and weather
        void ChangePressure(int changeValue);
    }
}
