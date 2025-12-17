using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Interfaces;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Server;

[Export(typeof(ITimeManager)), Shared]
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
    public string TimeInfo()
    {
        int day = Day + 1;

        string suffix;
        if (day > 4 && day < 20) suffix = "th";
        else if (day % 10 == 1) suffix = "st";
        else if (day % 10 == 2) suffix = "nd";
        else if (day % 10 == 3) suffix = "rd";
        else suffix = "th";

        return string.Format("It is {0} o'clock {1}, Day of {2}, {3}{4} the Month of {5}.",
            (Hour % 12 == 0) ? 12 : Hour % 12,
            Hour >= 12 ? "pm" : "am",
            _dayNames[day % 7],
            day,
            suffix,
            _monthNames[Month]);
    }

    // Weather
    public int Pressure { get; protected set; }
    public int PressureChange { get; protected set; }
    public SunPhases SunPhase { get; protected set; }
    public SkyStates SkyState { get; protected set; }

    // Moons
    public int MoonCount => 2;
    public int MoonPhase(int moonId) => (_moons[moonId].PhaseStart * MoonPhaseCount) / _moons[moonId].PhasePeriod;
    public bool IsMoonNight()=> Hour < 5 || Hour >= 20;
    public bool IsMoonInSky(int moonId) => _moons[moonId].PositionStart >= _moons[moonId].PositionVisibleFrom;
    public bool IsMoonVisible(int moonId) =>
        IsMoonNight()
        && MoonPhase(moonId) != 0
        && IsMoonInSky(moonId);
    public bool IsMoonFull(int moonId) => MoonPhase(moonId) == FullMoon;
    public string MoonInfo(int moonId) => string.Format(MoonPhaseMsg[MoonPhase(moonId)], _moons[moonId].Name);

    //
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

        StringBuilder sb = new ();
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

        UpdateMoons(sb);

        // weather change
        int diff;
        if (Month >= 9 && Month <= 16)
            diff = Pressure > 985 ? -2 : 2;
        else
            diff = Pressure > 1015 ? -2 : 2;
        PressureChange += diff * RandomManager.Dice(1, 4) + RandomManager.Dice(2, 6) - RandomManager.Dice(2, 6);
        PressureChange = Math.Clamp(PressureChange, -12, 12);
        Pressure += PressureChange;
        Pressure = Math.Clamp(Pressure, 960, 1040);
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

    private readonly string[] _dayNames =
    [
        "the Moon", "the Bull", "Deception", "Thunder", "Freedom",
        "the Great Gods", "the Sun"
    ];

    private readonly string[] _monthNames =
    [
        "Winter", "the Winter Wolf", "the Frost Giant", "the Old Forces",
        "the Grand Struggle", "the Spring", "Nature", "Futility", "the Dragon",
        "the Sun", "the Heat", "the Battle", "the Dark Shades", "the Shadows",
        "the Long Shadows", "the Ancient Darkness", "the Great Evil"
    ];

    //This is a pseudo moon simulator. It's totally 
    //false astronomically. But so is the rest of the mud...
    //Who cares?
    //Anyway, this implements moon phases and pseudo-rotation.
    //
    //Below the table indicates the data of the moons.
    //The two leftmost numbers indicates the phase data.
    //The second is the duration, in hours, of a full cycle.
    //The first is the start hour in this cycle. This is important
    //for prediction of moon conjunctions.
    //
    //The three next are the same but for position in the sky.
    //The middle number is the hour of moon rise, in the moon rotation
    //cycle.
    //
    //Example : 12*29, 24*29,	9, 10, 23,	"white"
    //
    //That means that a cycle is 29 days. The mud starts with moon full.
    //The moon day is 23 hours, so a little shift per mud day.
    //The moon is visible 12/23th of the time, and will be rising one hour
    //after the mud has started.
    private readonly MoonData[] _moons =
    [
        new MoonData(12*29, 24*29, 9, 10, 23, "blue"),
        new MoonData(12*13, 24*13, 0,  7, 14, "red")
    ];

    private class MoonData
    {
        // phase : start(ph_t), period(ph_p),
        public int PhaseStart { get; internal set; }
        public int PhasePeriod { get; }

        // position : start(po_t), visible from(po_v), period(po_p)
        public int PositionStart { get; internal set; }
        public int PositionVisibleFrom { get; internal set; }
        public int PositionPeriod { get; }

        public string Name { get; }

        public MoonData(int phaseStart, int phasePeriod, int positionStart, int positionVisibleFrom, int positionPeriod, string name)
        {
            PhaseStart = phaseStart;
            PhasePeriod = phasePeriod;
            PositionStart = positionStart;
            PositionVisibleFrom = positionVisibleFrom;
            PositionPeriod = positionPeriod;
            Name = name;
        }
    }

    private const int MoonPhaseCount = 8;
    private const int FullMoon = 4;

    private static readonly string[] MoonPhaseMsg =
    [
      "",
      "You see a crescent shaped growing {0} moon.",
      "You see the first quarter the {0} moon.",
      "The {0} moon is waning.",
      "The {0} moon is full and lightens the whole place.",
      "The {0} moon is waning.",
      "You see the last quarter of the {0} moon.",
      "The last crescent of the {0} moon appears in the sky."
    ];

    void UpdateMoons(StringBuilder sb)
    {
        // Moons changes.
        for (int i = 0; i < MoonCount; i++)
        {
            bool wasvis = IsMoonVisible(i);
            // update position and phase
            if (++_moons[i].PhaseStart >= _moons[i].PhasePeriod)
                _moons[i].PhaseStart = 0;
            if (++_moons[i].PositionStart >= _moons[i].PositionPeriod)
                _moons[i].PositionStart = 0;

            // if night and moon visibility has changed
            if (IsMoonVisible(i) != wasvis && IsMoonNight())
            {
                if (Hour == 20)
                    sb.AppendFormatLine("The {0} moon is fading through the night.", _moons[i].Name);
                else if (_moons[i].PositionStart == _moons[i].PositionVisibleFrom)
                    sb.AppendFormatLine("The {0} moon rises.", _moons[i].Name);
                else if (_moons[i].PositionStart == 0)
                    sb.AppendFormatLine("The {0} moon sets.", _moons[i].Name);
                else if (MoonPhase(i) == 1)
                    sb.AppendFormatLine("The {0} moon shows up a thin crescent.", _moons[i].Name);
                else if (MoonPhase(i) == 0)
                    sb.AppendFormatLine("The remaining crescent of the {0} moon has disappeared.", _moons[i].Name);
            }
        }
    }
}
