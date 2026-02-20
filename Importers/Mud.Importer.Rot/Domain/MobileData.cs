namespace Mud.Importer.Rot.Domain;

internal class MobileData
{
    public int VNum { get; set; }
    public string PlayerName { get; set; } = default!;
    public string ShortDescr { get; set; } = default!;
    public string LongDescr { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Race { get; set; } = default!;
    public long Act { get; set; }
    public long Act2 { get; set; }
    public long AffectedBy { get; set; }
    public long ShieldedBy { get; set; }
    public int Alignment { get; set; }
    public int Group { get; set; }
    public int Level { get; set; }
    public int HitRoll { get; set; }
    public int[] Hit { get; set; } = new int[3];// 0: DiceCount, 1: DiceValue, 2: DiceBonus
    public int[] Mana { get; set; } = new int[3]; // 0: DiceCount, 1: DiceValue, 2: DiceBonus
    public int[] Dam { get; set; } = new int[3]; // 0: DiceCount, 1: DiceValue, 2: DiceBonus
    public string DamType { get; set; } = default!;
    public int[] Armor { get; set; } = new int[4]; // 0: Pierce, 1: Bash, 2: Slash, 3: Exotic
    public long OffFlags { get; set; }
    public long ImmFlags { get; set; }
    public long ResFlags { get; set; }
    public long VulnFlags { get; set; }
    public string StartPos { get; set; } = default!;
    public string DefaultPos { get; set; } = default!;
    public string Sex { get; set; } = default!;
    public long Wealth { get; set; }
    public long Form { get; set; }
    public long Parts { get; set; }
    public string Size { get; set; } = default!;
    public string Material { get; set; } = default!;

    // 'D'
    public string DieDescr { get; set; } = default!;
    // 'T'
    public string SayDescr { get; set; } = default!;
    // 'M'
    public List<MobProgramTrigger> MobProgramTriggers { get; set; } = [];
    // 'C'
    public string Clan { get; set; } = default!;

    public ShopData Shop { get; set; } = default!;

    public string Special { get; set; } = default!;
}

public class MobProgramTrigger
{
    public string TrigType { get; set; } = default!;
    public int VNum { get; set; }
    public string TrigPhrase { get; set; } = default!;
}

public class ShopData
{
    public const int MaxTrades = 5;

    public int Keeper { get; set; } // mob vnum
    public int[] BuyType { get; set; } = new int[MaxTrades]; // item type
    public int ProfitBuy { get; set; }
    public int ProfitSell { get; set; }
    public int OpenHour { get; set; }
    public int CloseHour { get; set; }
}
