namespace Mud.Importer.Mystery.Domain;

internal class RoomData
{
    public const int MaxExits = 11;

    public int AreaVnum { get; set; }

    public int VNum { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public long Flags { get; set; } // flags bstat(flags)
    public int Sector { get; set; } // bstat(sector)
    public int MaxSize { get; set; } // bstat(maxsize)
    public int HealRate { get; set; } = 100; // bstat(healrate)
    public int ManaRate { get; set; } = 100; // bstat(manarate)
    public int PspRate { get; set; } = 100; // bstat(psprate)
    public string Clan { get; set; } = default!;
    public ExitData[] Exits { get; set; } = new ExitData[MaxExits];
    public Dictionary<string, string> ExtraDescr { get; set; } = []; // keyword -> description
    public string Owner { get; set; } = default!;
    public long Guilds { get; set; } // flags
    public string Program { get; set; } = default!; // Room program name
    public int TimeBetweenRepop { get; set; }
    public int TimeBetweenRepopPeople { get; set; }
    public List<ResetData> Resets { get; set; } = [];
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

internal class ExitData
{
    public string Description { get; set; } = default!;
    public string Keyword { get; set; } = default!;
    public long ExitInfo { get; set; }
    public int Key { get; set; } // key object vnum
    public int DestinationVNum { get; set; } // destination room vnum  (u1.vnum in original code)
}

internal class ResetData
{
    public char Command { get; set; }
    public int Arg1 { get; set; }
    public int Arg2 { get; set; }
    public int Arg3 { get; set; }
    public int Arg4 { get; set; }
}
