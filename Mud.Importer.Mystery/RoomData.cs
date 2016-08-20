using System.Collections.Generic;

namespace Mud.Importer.Mystery
{
    public class RoomData
    {
        public const int MaxExits = 11;

        public int AreaVnum { get; set; }

        public int VNum { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long Flags { get; set; } // flags bstat(flags)
        public int Sector { get; set; } // bstat(sector)
        public int MaxSize { get; set; } // bstat(maxsize)
        public int HealRate { get; set; } // bstat(healrate)
        public int ManaRate { get; set; } // bstat(manarate)
        public int PspRate { get; set; } // bstat(psprate)
        public string Clan { get; set; }
        public ExitData[] Exits { get; set; }
        public Dictionary<string, string> ExtraDescr; // keyword -> description
        public string Owner { get; set; }
        public long Guilds { get; set; } // flags
        public string Program { get; set; } // Room program name
        public int TimeBetweenRepop { get; set; }
        public int TimeBetweenRepopPeople { get; set; }
        public List<ResetData> Resets { get; set; }

        public RoomData()
        {
            Exits = new ExitData[MaxExits];
            ExtraDescr = new Dictionary<string, string>();
            Resets = new List<ResetData>();
            // Default values
            HealRate = 100;
            ManaRate = 100;
            PspRate = 100;
        }
    }
    // Door id
    // 0: NORTH
    // 1: EAST
    // 2: SOUTH
    // 3: WEST
    // 4: UP
    // 5: DOWN
    // 6: NORTHEAST
    // 7: NORTHWEST
    // 8: SOUTHEAST
    // 9: SOUTHWEST
    // 10: SPECIAL

    public class ExitData
    {
        public string Description { get; set; }
        public string Keyword { get; set; }
        public long ExitInfo { get; set; }
        public int Key { get; set; } // key object vnum
        public int DestinationVNum { get; set; } // destination room vnum  (u1.vnum in original code)
    }

    public class ResetData
    {
        public char Command { get; set; }
        public int Arg1 { get; set; }
        public int Arg2 { get; set; }
        public int Arg3 { get; set; }
        public int Arg4 { get; set; }
    }
}
