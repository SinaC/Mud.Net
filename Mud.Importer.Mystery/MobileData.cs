namespace Mud.Importer.Mystery;

public class MobileData
{
    public int VNum { get; set; }
    public string Name { get; set; } = default!;
    public string ShortDescr { get; set; } = default!;
    public string LongDescr { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Race { get; set; } = default!; // lookup table
    public long Classes { get; set; } // flags
    public long Act { get; set; } // flags
    public long AffectedBy { get; set; } // flags
    public long AffectedBy2 { get; set; } // flags
    public int Etho { get; set; } // -1: chaotic  0: neutral  1: lawful
    public int Alignment { get; set; } // -1000: evil -> +1000: good
    public int Group { get; set; }
    public int Level { get; set; }
    public int HitRoll { get; set; }
    public int[] Hit { get; set; } = new int[3]; // Hit points: 0: DiceNumber, 1: DiceType, 2: DiceBonus
    public int[] Mana { get; set; } = new int[3]; // Mana: 0: DiceNumber, 1: DiceType, 2: DiceBonus
    public int[] Psp { get; set; } = new int[3]; // Psionic points: 0: DiceNumber, 1: DiceType, 2: DiceBonus
    public int[] Damage { get; set; } = new int[3]; // Damage: 0: DiceNumber, 1: DiceType, 2: DiceBonus
    public string DamageType { get; set; } = default!; // lookup table
    public int[] Armor { get; set; } = new int[4]; // 0: Pierce, 1: Bash, 2: Slash, 3: Exotic
    public long OffFlags { get; set; } // flags
    public long ImmFlags { get; set; } // flags
    public long ResFlags { get; set; } // flags
    public long VulnFlags { get; set; } // flags
    public string StartPos { get; set; } = default!; // starting position  lookup table
    public string DefaultPos { get; set; } = default!; // default position  lookup table
    public string Sex { get; set; } = default!; // lookup table
    public long Wealth { get; set; } // gold
    public long Form { get; set; } // flags
    public long Parts { get; set; } // flags
    public string Size { get; set; } = default!; // lookup table
    public string Material { get; set; } = default!;
    public string Program { get; set; } = default!; // Mob program name
    public string Special { get; set; } = default!; // Read from #SPECIALS and not from #MOBILES
    public ShopData Shop { get; set; } = default!;
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

public enum AffectedBy
{
    AFF_BLIND = MysteryLoader.A,
    AFF_INVISIBLE = MysteryLoader.B,
    AFF_DETECT_EVIL = MysteryLoader.C,
    AFF_DETECT_INVIS = MysteryLoader.D,
    AFF_DETECT_MAGIC = MysteryLoader.E,
    AFF_DETECT_HIDDEN = MysteryLoader.F,
    AFF_DETECT_GOOD = MysteryLoader.G,
    AFF_SANCTUARY = MysteryLoader.H,
    AFF_FAERIE_FIRE = MysteryLoader.I,
    AFF_INFRARED = MysteryLoader.J,
    AFF_CURSE = MysteryLoader.K,
    // Modified by SinaC 2001
    //#define AFF_UNUSED_FLAG		 = MysteryImporter.L,	/* unused */
    AFF_ROOTED = MysteryLoader.L,
    AFF_POISON = MysteryLoader.M,
    AFF_PROTECT_EVIL = MysteryLoader.N,
    AFF_PROTECT_GOOD = MysteryLoader.O,
    AFF_SNEAK = MysteryLoader.P,
    AFF_HIDE = MysteryLoader.Q,
    AFF_SLEEP = MysteryLoader.R,
    AFF_CHARM = MysteryLoader.S,
    AFF_FLYING = MysteryLoader.T,
    AFF_PASS_DOOR = MysteryLoader.U,
    AFF_HASTE = MysteryLoader.V,
    AFF_CALM = MysteryLoader.W,
    AFF_PLAGUE = MysteryLoader.X,
    AFF_WEAKEN = MysteryLoader.Y,
    AFF_DARK_VISION = MysteryLoader.Z,
    AFF_BERSERK = MysteryLoader.aa,
    AFF_SWIM = MysteryLoader.bb,
    AFF_REGENERATION = MysteryLoader.cc,
    AFF_SLOW = MysteryLoader.dd,
    // Added by SinaC 2000 for SILENCED people, can't cast spell
    AFF_SILENCE = MysteryLoader.ee,
}

public enum AffectedBy2
{
    AFF2_WALK_ON_WATER = MysteryLoader.A,
    AFF2_WATER_BREATH = MysteryLoader.B,
    AFF2_DETECT_EXITS = MysteryLoader.C,
    AFF2_MAGIC_MIRROR = MysteryLoader.D,
    AFF2_FAERIE_FOG = MysteryLoader.E,
    AFF2_NOEQUIPMENT = MysteryLoader.F,
    // Added by SinaC 2003
    AFF2_FREE_MOVEMENT = MysteryLoader.G,
    AFF2_INCREASED_CASTING = MysteryLoader.H,
    AFF2_NOSPELL = MysteryLoader.I,
    AFF2_NECROTISM = MysteryLoader.J,
    AFF2_HIGHER_MAGIC_ATTRIBUTES = MysteryLoader.K,
    AFF2_CONFUSION = MysteryLoader.L,
}

public enum Act
{
    ACT_IS_NPC = MysteryLoader.A,     /* Auto set for mobs	*/
    ACT_SENTINEL = MysteryLoader.B,       /* Stays in one room	*/
    ACT_SCAVENGER = MysteryLoader.C,      /* Picks up objects	*/
    ACT_AWARE = MysteryLoader.E,             // can't be backstab
    ACT_AGGRESSIVE = MysteryLoader.F,         /* Attacks PC's		*/
    ACT_STAY_AREA = MysteryLoader.G,      /* Won't leave area	*/
    ACT_WIMPY = MysteryLoader.H,
    ACT_PET = MysteryLoader.I,        /* Auto set for pets	*/
    ACT_TRAIN = MysteryLoader.J,      /* Can train PC's	*/
    ACT_PRACTICE = MysteryLoader.K,       /* Can practice PC's	*/
    // Added by SinaC 2001
    ACT_FREE_WANDER = MysteryLoader.L,  /* Can leave an area without being extract, SinaC 2001 */
    // Added by SinaC 2003, ACT_MOUNTABLE tells if a mob can be mounted using mount/dismount command
    // ACT_IS_MOUNTED tells if a mob is mounted
    ACT_MOUNTABLE = MysteryLoader.M,
    ACT_IS_MOUNTED = MysteryLoader.N,
    ACT_UNDEAD = MysteryLoader.O,
    ACT_NOSLEEP = MysteryLoader.P,
    ACT_CLERIC = MysteryLoader.Q,
    ACT_MAGE = MysteryLoader.R,
    ACT_THIEF = MysteryLoader.S,
    ACT_WARRIOR = MysteryLoader.T,
    ACT_NOALIGN = MysteryLoader.U,
    ACT_NOPURGE = MysteryLoader.V,
    ACT_OUTDOORS = MysteryLoader.W,
    ACT_INDOORS = MysteryLoader.Y,
    // Added by SinaC 2003, set if the mob has been created with an ability such as summon elemental, ...
    ACT_CREATED = MysteryLoader.Z,
    ACT_IS_HEALER = MysteryLoader.aa,
    ACT_GAIN = MysteryLoader.bb,
    ACT_UPDATE_ALWAYS = MysteryLoader.cc,
    //#define ACT_IS_CHANGER		 = MysteryImporter.dd,
    // Added by SinaC 2003
    // ACT_RESERVED is used to mark a mob from script, so that mob can't be a quest target
    ACT_RESERVED = MysteryLoader.dd,
    ACT_IS_SAFE = MysteryLoader.ee,
}

public enum Offensive
{
    OFF_AREA_ATTACK = MysteryLoader.A,
    OFF_BACKSTAB = MysteryLoader.B,
    OFF_BASH = MysteryLoader.C,
    OFF_BERSERK = MysteryLoader.D,
    OFF_DISARM = MysteryLoader.E,
    OFF_DODGE = MysteryLoader.F,
    OFF_FADE = MysteryLoader.G,
    OFF_FAST = MysteryLoader.H,
    OFF_KICK = MysteryLoader.I,
    OFF_KICK_DIRT = MysteryLoader.J,
    OFF_PARRY = MysteryLoader.K,
    OFF_RESCUE = MysteryLoader.L,
    OFF_TAIL = MysteryLoader.M,
    OFF_TRIP = MysteryLoader.N,
    OFF_CRUSH = MysteryLoader.O,

    ASSIST_ALL = MysteryLoader.P,
    ASSIST_ALIGN = MysteryLoader.Q,
    ASSIST_RACE = MysteryLoader.R,
    ASSIST_PLAYERS = MysteryLoader.S,
    ASSIST_GUARD = MysteryLoader.T,
    ASSIST_VNUM = MysteryLoader.U,
    // Added by SinaC 2000 to add some fun :,,,, counter-attack for mobiles
    OFF_COUNTER = MysteryLoader.V,
    OFF_BITE = MysteryLoader.W,
}
