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
        public Dictionary<string, string> ExtraDescr { get; set; } // keyword -> description
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

    public enum ResetDataWearLocation
    {
        WEAR_NONE = -1,
        WEAR_LIGHT = 0,
        WEAR_FINGER_L = 1,
        WEAR_FINGER_R = 2,
        WEAR_NECK_1 = 3,
        WEAR_NECK_2 = 4,
        WEAR_BODY = 5,
        WEAR_HEAD = 6,
        WEAR_LEGS = 7,
        WEAR_FEET = 8,
        WEAR_HANDS = 9,
        WEAR_ARMS = 10,
        WEAR_SHIELD = 11,
        WEAR_ABOUT = 12,
        WEAR_WAIST = 13,
        WEAR_WRIST_L = 14,
        WEAR_WRIST_R = 15,
        WEAR_WIELD = 16,
        WEAR_HOLD = 17,
        WEAR_FLOAT = 18,
        MAX_WEAR = 19,
    }

    public enum SectorTypes
    {
        SECT_INSIDE		    = 0,
        SECT_CITY		    = 1,
        SECT_FIELD		    = 2,
        SECT_FOREST		    = 3,
        SECT_HILLS		    = 4,
        SECT_MOUNTAIN		= 5,
        SECT_WATER_SWIM		= 6,
        SECT_WATER_NOSWIM	= 7,
        SECT_BURNING		= 8,
        SECT_AIR		    = 9,
        SECT_DESERT		    = 10,
        SECT_UNDERWATER     = 11,
    }
}
