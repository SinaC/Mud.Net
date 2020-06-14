using System.Collections.Generic;

namespace Mud.Importer.Rot
{
    public class RoomData
    {
        public const int MaxDir = 10;

        public int AreaVnum { get; set; }

        public int VNum { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long RoomFlags { get; set; }
        public int SectorType { get; set; }
        public int HealRate { get; set; }
        public int ManaRate { get; set; }
        public string Clan { get; set; }
        public string Guild { get; set; }
        public string Race { get; set; }
        public ExitData[] Exits { get; set; }
        public Dictionary<string,string> ExtraDescr { get; set; }
        public ExitData Transfer { get; set; }
        public string Owner { get; set; }
        public bool HealNeg { get; set; }

        public List<ResetData> Resets { get; set; }

        public RoomData()
        {
            Exits = new ExitData[MaxDir];
            ExtraDescr = new Dictionary<string, string>();
            Resets = new List<ResetData>();
        }
    }

    public class ExitData
    {
        public const long ExDoor = 0x01;
        public const long ExClosed = 0x02;
        public const long ExLocked = 0x04;
        public const long ExPickproof = 0x08;
        public const long ExNoPass = 0x10;

        public string Description { get; set; }
        public string Keyword { get; set; }
        public long ExitInfo { get; set; }
        public long RsFlags { get; set; }
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
