using System;
using System.Text;
using Mud.Domain;
using Mud.Server.Common;

namespace Mud.Server.Server
{
    public class TimeManager : ITimeManager
    {
        private IRandomManager RandomManager { get; }
        private bool _initialized;

        public TimeManager(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        #region ITimeManager

        // Centralized time synchronized on server's pulse
        public DateTime CurrentTime { get; protected set; }

        // World' specific time values
        public int Hour { get; protected set; }
        public int Day { get; protected set; }
        public int Month { get; protected set; }
        public int Year { get; protected set; }

        public int Pressure { get; protected set; }

        public int PressureChange { get; protected set; }

        public SunPhases SunPhase { get; protected set; }

        public SkyStates SkyState { get; protected set; }

        public void Initialize()
        {
            if (_initialized)
                return;
            CurrentTime = DateTime.Now;

            long seconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            long hour = (seconds - 650336715) / 60;
            long day = hour / 24;
            long month = day / 35;
            Hour = (int)(hour % 24);
            Day = (int)(day % 35);
            Month = (int)(month % 17);
            Year = (int)(month / 17);

            if (Hour < 5) SunPhase = SunPhases.Dark;
            else if (Hour < 6) SunPhase = SunPhases.Rise;
            else if (Hour < 19) SunPhase = SunPhases.Light;
            else if (Hour < 20) SunPhase = SunPhases.Set;
            else SunPhase = SunPhases.Dark;

            PressureChange = 0;
            Pressure = 960;
            if (Month >= 7 && Month <= 12)
                Pressure += RandomManager.Range(1, 50);
            else
                Pressure += RandomManager.Range(1, 80);

            if (Pressure <= 980) SkyState = SkyStates.Lightning;
            else if (Pressure <= 1000) SkyState = SkyStates.Raining;
            else if (Pressure <= 1020) SkyState = SkyStates.Cloudy;
            else SkyState = SkyStates.Cloudless;

            _initialized = true;
        }

        public void FixCurrentTime()
        {
            CurrentTime = DateTime.Now;
            if (!_initialized)
                Initialize();
        }

        public string Update()
        {
            if (!_initialized)
                Initialize();

            StringBuilder sb = new StringBuilder();
            // time change
            Hour++;
            switch (Hour)
            {
                case 5:
                    sb.AppendLine("The day has begun.");
                    SunPhase = SunPhases.Light;
                    break;
                case 6:
                    sb.AppendLine("The sun rises in the east.");
                    SunPhase = SunPhases.Rise;
                    break;
                case 19:
                    sb.AppendLine("The sun slowly disappears in the west.");
                    SunPhase = SunPhases.Set;
                    break;
                case 20:
                    sb.AppendLine("The night has begun.");
                    SunPhase = SunPhases.Dark;
                    break;
                case 24:
                    // next day
                    Hour = 0;
                    Day++;
                    break;
            }
            // next month
            if (Day >= 35)
            {
                Day = 0;
                Month++;
            }
            // next year
            if (Month >= 17)
            {
                Day = 0;
                Month = 0;
                Year++;
            }

            // weather change
            int diff;
            if (Month >= 9 && Month <= 16)
                diff = Pressure > 985 ? -2 : 2;
            else
                diff = Pressure > 1015 ? -2 : 2;
            PressureChange += diff * RandomManager.Dice(1, 4) + RandomManager.Dice(2, 6) - RandomManager.Dice(2, 6);
            PressureChange = PressureChange.Range(-12, 12);
            Pressure += PressureChange;
            Pressure = Pressure.Range(960, 1040);
            switch (SkyState)
            {
                case SkyStates.Cloudless:
                    if (Pressure < 990
                        || (Pressure < 1010 && RandomManager.Chance(25)))
                    {
                        sb.AppendLine("The sky is getting cloudy.");
                        SkyState = SkyStates.Cloudy;
                    }

                    break;
                case SkyStates.Cloudy:
                    if (Pressure < 970
                        || (Pressure < 990 && RandomManager.Chance(25)))
                    {
                        sb.AppendLine("It starts to rain.");
                        SkyState = SkyStates.Raining;
                    }
                    else if (Pressure > 1030 && RandomManager.Chance(25))
                    {
                        sb.AppendLine("The clouds disappear.");
                        SkyState = SkyStates.Cloudless;
                    }

                    break;
                case SkyStates.Raining:
                    if (Pressure < 970 && RandomManager.Chance(25))
                    {
                        sb.AppendLine("Lightning flashes in the sky.");
                        SkyState = SkyStates.Lightning;
                    }
                    else if (Pressure > 1030
                             || (Pressure > 1010 && RandomManager.Chance(25)))
                    {
                        sb.AppendLine("The rain stopped.");
                        SkyState = SkyStates.Cloudy;
                    }

                    break;
                case SkyStates.Lightning:
                    if (Pressure > 1010
                        || (Pressure > 990 && RandomManager.Chance(25)))
                    {
                        sb.AppendLine("The lightning has stopped.");
                        SkyState = SkyStates.Raining;
                    }

                    break;
            }

            return sb.ToString();
        }

        public void ChangePressure(int changeValue)
        {
            PressureChange += changeValue;
        }


        #endregion
    }
}
