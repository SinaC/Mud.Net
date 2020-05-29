using System.Collections.Generic;

namespace Mud.Importer.Rot
{
    public class MobileData
    {
        public int VNum { get; set; }
        public string PlayerName { get; set; }
        public string ShortDescr { get; set; }
        public string LongDescr { get; set; }
        public string Description { get; set; }
        public string Race { get; set; }
        public long Act { get; set; }
        public long Act2 { get; set; }
        public long AffectedBy { get; set; }
        public long ShieldedBy { get; set; }
        public int Alignment { get; set; }
        public int Group { get; set; }
        public int Level { get; set; }
        public int HitRoll { get; set; }
        public int[] Hit { get; set; } // 0: DiceCount, 1: DiceValue, 2: DiceBonus
        public int[] Mana { get; set; } // 0: DiceCount, 1: DiceValue, 2: DiceBonus
        public int[] Dam { get; set; } // 0: DiceCount, 1: DiceValue, 2: DiceBonus
        public string DamType { get; set; }
        public int[] Armor { get; set; } // 0: Pierce, 1: Bash, 2: Slash, 3: Exotic
        public long OffFlags { get; set; }
        public long ImmFlags { get; set; }
        public long ResFlags { get; set; }
        public long VulnFlags { get; set; }
        public string StartPos { get; set; }
        public string DefaultPos { get; set; }
        public string Sex { get; set; }
        public long Wealth { get; set; }
        public long Form { get; set; }
        public long Parts { get; set; }
        public string Size { get; set; }
        public string Material { get; set; }

        // 'D'
        public string DieDescr { get; set; }
        // 'T'
        public string SayDescr { get; set; }
        // 'M'
        public List<MobProg> MobProgs {get;set;}
        // 'C'
        public string Clan { get; set; }

        public ShopData Shop { get; set; }

        public string Special { get; set; }

        public MobileData()
        {
            Hit = new int[3];
            Mana = new int[3];
            Dam = new int[3];
            Armor = new int[4];
            MobProgs = new List<MobProg>();
        }
    }

    public class MobProg
    {
        // Read from #MOBILES
        public string TrigType { get; set; }
        public int VNum { get; set; }
        public string TrigPhrase { get; set; }
        // Read from #MOBPROGS
        public string Code { get; set; }
    }

    public class ShopData
    {
        public const int MaxTrades = 5;

        public int Keeper { get; set; } // mob vnum
        public int[] BuyType { get; set; } // item type
        public int ProfitBuy { get; set; }
        public int ProfitSell { get; set; }
        public int OpenHour { get; set; }
        public int CloseHour { get; set; }

        public ShopData()
        {
            BuyType = new int[MaxTrades];
        }
    }
}
