namespace Mud.Importer.Rot.Domain;

internal class RoomData
{
    public const int MaxDir = 10;

    public int AreaVnum { get; set; }

    public int VNum { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public long RoomFlags { get; set; }
    public int SectorType { get; set; }
    public int HealRate { get; set; }
    public int ManaRate { get; set; }
    public string Clan { get; set; } = default!;
    public string Guild { get; set; } = default!;
    public string Race { get; set; } = default!;
    public ExitData[] Exits { get; set; } = new ExitData[MaxDir];
    public Dictionary<string, string> ExtraDescr { get; set; } = [];
    public ExitData Transfer { get; set; } = default!;
    public string Owner { get; set; } = default!;
    public bool HealNeg { get; set; }

    public List<ResetData> Resets { get; set; } = [];
}

internal class ExitData
{
    public const long ExDoor = 0x01;
    public const long ExClosed = 0x02;
    public const long ExLocked = 0x04;
    public const long ExPickproof = 0x20;
    public const long ExNoPass = 0x40;

    public string Description { get; set; } = default!;
    public string Keyword { get; set; } = default!;
    public long ExitInfo { get; set; }
    public long RsFlags { get; set; }
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
