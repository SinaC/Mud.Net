namespace Mud.Importer.Mystery
{
    public class MobileData
    {
        public int VNum { get; set; }
        public string Name { get; set; }
        public string ShortDescr { get; set; }
        public string LongDescr { get; set; }
        public string Description { get; set; }
        public string Race { get; set; } // lookup table
        public long Classes { get; set; } // flags
        public long Act { get; set; } // flags
        public long AffectedBy { get; set; } // flags
        public long AffectedBy2 { get; set; } // flags
        public int Etho { get; set; } // -1: chaotic  0: neutral  1: lawful
        public int Alignment { get; set; } // -1000: evil -> +1000: good
        public int Group { get; set; }
        public int Level { get; set; }
        public int HitRoll { get; set; }
        public int[] Hit { get; set; } // Hit points: 0: DiceNumber, 1: DiceType, 2: DiceBonus
        public int[] Mana { get; set; } // Mana: 0: DiceNumber, 1: DiceType, 2: DiceBonus
        public int[] Psp { get; set; } // Psionic points: 0: DiceNumber, 1: DiceType, 2: DiceBonus
        public int[] Damage { get; set; } // Damage: 0: DiceNumber, 1: DiceType, 2: DiceBonus
        public string DamageType { get; set; } // lookup table
        public int[] Armor { get; set; } // 0: Pierce, 1: Bash, 2: Slash, 3: Exotic
        public long OffFlags { get; set; } // flags
        public long ImmFlags { get; set; } // flags
        public long ResFlags { get; set; } // flags
        public long VulnFlags { get; set; } // flags
        public string StartPos { get; set; } // starting position  lookup table
        public string DefaultPos { get; set; } // default position  lookup table
        public string Sex { get; set; } // lookup table
        public long Wealth { get; set; } // gold
        public long Form { get; set; } // flags
        public long Parts { get; set; } // flags
        public string Size { get; set; } // lookup table
        public string Material { get; set; }
        public string Program { get; set; } // Mob program name
        public string Special { get; set; } // Read from #SPECIALS and not from #MOBILES
        public ShopData Shop { get; set; }

        public MobileData()
        {
            Hit = new int[3];
            Mana = new int[3];
            Psp = new int[3];
            Damage = new int[3];
            Armor = new int[4];
        }
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

    // position table
    //---------------
    //{	"dead",			"dead"	},
    //{	"mortally wounded",	"mort"	},
    //{	"incapacitated",	"incap"	},
    //{   "paralyzed",            "para"  }, // Added by SinaC 2003
    //{	"stunned",		"stun"	},
    //{	"sleeping",		"sleep"	},
    //{	"resting",		"rest"	},
    //{   "sitting",		"sit"   },
    //{	"fighting",		"fight"	},
    //{	"standing",		"stand"	},

    //sex table
    //---------
    //{	"none"		},
    //{	"male"		},
    //{	"female"	},
    //{	"either"	},

    // size table
    //-----------
    //{	"tiny"		},
    //{	"small" 	},
    //{	"medium"	},
    //{	"large"		},
    //{	"huge", 	},
    //{	"giant" 	},

    // act table
    //----------
    //#define ACT_IS_NPC		(A)		/* Auto set for mobs	*/
    //#define ACT_SENTINEL	    	(B)		/* Stays in one room	*/
    //#define ACT_SCAVENGER	      	(C)		/* Picks up objects	*/
    //#define ACT_AWARE               (E)             // can't be backstab
    //#define ACT_AGGRESSIVE		(F)    		/* Attacks PC's		*/
    //#define ACT_STAY_AREA		(G)		/* Won't leave area	*/
    //#define ACT_WIMPY		(H)
    //#define ACT_PET			(I)		/* Auto set for pets	*/
    //#define ACT_TRAIN		(J)		/* Can train PC's	*/
    //#define ACT_PRACTICE		(K)		/* Can practice PC's	*/
    //// Added by SinaC 2001
    //#define ACT_FREE_WANDER         (L)  /* Can leave an area without being extract, SinaC 2001 */
    //// Added by SinaC 2003, ACT_MOUNTABLE tells if a mob can be mounted using mount/dismount command
    //// ACT_IS_MOUNTED tells if a mob is mounted
    //#define ACT_MOUNTABLE           (M)
    //#define ACT_IS_MOUNTED          (N)
    //#define ACT_UNDEAD		(O)	
    //#define ACT_NOSLEEP             (P)
    //#define ACT_CLERIC		(Q)
    //#define ACT_MAGE		(R)
    //#define ACT_THIEF		(S)
    //#define ACT_WARRIOR		(T)
    //#define ACT_NOALIGN		(U)
    //#define ACT_NOPURGE		(V)
    //#define ACT_OUTDOORS		(W)
    //#define ACT_INDOORS		(Y)
    //#define ACT_CREATED             (Z)
    //#define ACT_IS_HEALER		(aa)
    //#define ACT_GAIN		(bb)
    //#define ACT_UPDATE_ALWAYS	(cc)
    //// ACT_RESERVED is used to mark a mob from script, so that mob can't be a quest target
    //#define ACT_RESERVED		(dd)
    //#define ACT_IS_SAFE		(ee)
    //  {	"npc",			ACT_IS_NPC,	FALSE	},
    //  {	"sentinel",		ACT_SENTINEL,	TRUE	},
    //  {	"scavenger",		ACT_SCAVENGER,	TRUE	},
    //  {   "aware",                ACT_AWARE,      TRUE    },
    //  {	"aggressive",		ACT_AGGRESSIVE,	TRUE	},
    //  {	"stay_area",		ACT_STAY_AREA,	TRUE	},
    //  {	"wimpy",		ACT_WIMPY,	TRUE	},
    //  {	"pet",			ACT_PET,	TRUE	},
    //  {	"train",		ACT_TRAIN,	TRUE	},
    //  {	"practice",		ACT_PRACTICE,	TRUE	},
    //  {   "freewander",           ACT_FREE_WANDER,TRUE    },
    //  {   "mountable",            ACT_MOUNTABLE,  TRUE    },
    //  {   "mounted",              ACT_IS_MOUNTED, TRUE    },
    //  {	"undead",		ACT_UNDEAD,	TRUE	},
    //  {   "nosleep",              ACT_NOSLEEP,    TRUE    },
    //  {	"cleric",		ACT_CLERIC,	TRUE	},
    //  {	"mage",			ACT_MAGE,	TRUE	},
    //  {	"thief",		ACT_THIEF,	TRUE	},
    //  {	"warrior",		ACT_WARRIOR,	TRUE	},
    //  {	"noalign",		ACT_NOALIGN,	TRUE	},
    //  {	"nopurge",		ACT_NOPURGE,	TRUE	},
    //  {	"outdoors",		ACT_OUTDOORS,	TRUE	},
    //  {	"indoors",		ACT_INDOORS,	TRUE	},
    //  {	"healer",		ACT_IS_HEALER,	TRUE	},
    //  {	"gain",			ACT_GAIN,	TRUE	},
    //  {	"update_always",	ACT_UPDATE_ALWAYS,	TRUE	},
    //  {   "reserved",             ACT_RESERVED,   TRUE   },
    //  {	"is_safe",		ACT_IS_SAFE,	TRUE	},
    //  {   "created",              ACT_CREATED,    TRUE    },

    // affectedBy table
    //-----------------
    //#define AFF_BLIND		(A)
    //#define AFF_INVISIBLE		(B)
    //#define AFF_DETECT_EVIL		(C)
    //#define AFF_DETECT_INVIS	(D)
    //#define AFF_DETECT_MAGIC	(E)
    //#define AFF_DETECT_HIDDEN	(F)
    //#define AFF_DETECT_GOOD		(G)
    //#define AFF_SANCTUARY		(H)
    //#define AFF_FAERIE_FIRE		(I)
    //#define AFF_INFRARED		(J)
    //#define AFF_CURSE		(K)
    //#define AFF_ROOTED              (L)
    //#define AFF_POISON		(M)
    //#define AFF_PROTECT_EVIL	(N)
    //#define AFF_PROTECT_GOOD	(O)
    //#define AFF_SNEAK		(P)
    //#define AFF_HIDE		(Q)
    //#define AFF_SLEEP		(R)
    //#define AFF_CHARM		(S)
    //#define AFF_FLYING		(T)
    //#define AFF_PASS_DOOR		(U)
    //#define AFF_HASTE		(V)
    //#define AFF_CALM		(W)
    //#define AFF_PLAGUE		(X)
    //#define AFF_WEAKEN		(Y)
    //#define AFF_DARK_VISION		(Z)
    //#define AFF_BERSERK		(aa)
    //#define AFF_SWIM		(bb)
    //#define AFF_REGENERATION        (cc)
    //#define AFF_SLOW		(dd)
    //#define AFF_SILENCE             (ee)
    //{	"blind",		AFF_BLIND,	        TRUE	},
    //{	"invisible",		AFF_INVISIBLE,	        TRUE	},
    //{	"detect_evil",		AFF_DETECT_EVIL,	TRUE	},
    //{	"detect_invis",		AFF_DETECT_INVIS,	TRUE	},
    //{	"detect_magic",		AFF_DETECT_MAGIC,	TRUE	},
    //{	"detect_hidden",	AFF_DETECT_HIDDEN,	TRUE	},
    //{	"detect_good",		AFF_DETECT_GOOD,	TRUE	},
    //{	"sanctuary",		AFF_SANCTUARY,	        TRUE	},
    //{	"faerie_fire",		AFF_FAERIE_FIRE,	TRUE	},
    //{	"infrared",		AFF_INFRARED,	        TRUE	},
    //{	"curse",		AFF_CURSE,      	TRUE	},
    //{	"poison",		AFF_POISON,     	TRUE	},
    //{	"protect_evil",		AFF_PROTECT_EVIL,	TRUE	},
    //{	"protect_good",		AFF_PROTECT_GOOD,	TRUE	},
    //{	"sneak",		AFF_SNEAK,      	TRUE	},
    //{	"hide",			AFF_HIDE,       	TRUE	},
    //{	"sleep",		AFF_SLEEP,	        TRUE	},
    //{	"charm",		AFF_CHARM,       	TRUE	},
    //{	"flying",		AFF_FLYING,        	TRUE	},
    //{	"pass_door",		AFF_PASS_DOOR,     	TRUE	},
    //{	"haste",		AFF_HASTE,       	TRUE	},
    //{	"calm",			AFF_CALM,        	TRUE	},
    //{	"plague",		AFF_PLAGUE,       	TRUE	},
    //{	"weaken",		AFF_WEAKEN,       	TRUE	},
    //{	"dark_vision",		AFF_DARK_VISION,	TRUE	},
    //{	"berserk",		AFF_BERSERK,      	TRUE	},
    //{	"swim",			AFF_SWIM,       	TRUE	},
    //{	"regeneration",		AFF_REGENERATION,	TRUE	},
    //{	"slow",			AFF_SLOW,       	TRUE	},
    //{   "silence",              AFF_SILENCE,            TRUE },
    //{   "rooted",               AFF_ROOTED,             TRUE },

    // affected2By table
    //#define AFF2_WALK_ON_WATER      (A)
    //#define AFF2_WATER_BREATH       (B)
    //#define AFF2_DETECT_EXITS       (C)
    //#define AFF2_MAGIC_MIRROR       (D)
    //#define AFF2_FAERIE_FOG         (E)
    //#define AFF2_NOEQUIPMENT        (F)
    //#define AFF2_FREE_MOVEMENT      (G)
    //#define AFF2_INCREASED_CASTING  (H)
    //#define AFF2_NOSPELL            (I)
    //#define AFF2_NECROTISM          (J)
    //#define AFF2_HIGHER_MAGIC_ATTRIBUTES (K)
    //#define AFF2_CONFUSION          (L)
    //{   "walk_on_water",        AFF2_WALK_ON_WATER,      TRUE },
    //{   "water_breath",         AFF2_WATER_BREATH,       TRUE },
    //{   "detect_exits",         AFF2_DETECT_EXITS,       TRUE },
    //{   "magic_mirror",         AFF2_MAGIC_MIRROR,       TRUE },
    //{   "faerie_fog",           AFF2_FAERIE_FOG,         TRUE },
    //{   "no_equipment",         AFF2_NOEQUIPMENT,        TRUE },
    //{   "free_movement",        AFF2_FREE_MOVEMENT,      TRUE },
    //{   "increased_casting",    AFF2_INCREASED_CASTING,  TRUE },
    //{   "no_spell",             AFF2_NOSPELL,            TRUE },
    //{   "necrotism",            AFF2_NECROTISM,          TRUE },
    //{   "higher_attributes",    AFF2_HIGHER_MAGIC_ATTRIBUTES, TRUE },
    //{   "confusion",            AFF2_CONFUSION,          TRUE },

    // afto_type table
    //----------------
//    {   "char",          AFTO_CHAR,            TRUE    },
//    {   "object",        AFTO_OBJECT,          TRUE    },
//    {   "objval",        AFTO_OBJVAL,          TRUE    },
//    {   "weapon",        AFTO_WEAPON,          TRUE    },
//    {   "room",          AFTO_ROOM,            TRUE    }, // SinaC 2003
//    {   NULL,              0,                    0       }
    // where definitions
    //#define AFTO_CHAR       0
    //#define AFTO_OBJECT     1
    //#define AFTO_WEAPON     2
    //#define AFTO_OBJVAL     3
    //#define AFTO_ROOM       4

    // op definitions
    //#define AFOP_ADD        0
    //#define AFOP_OR         1
    //#define AFOP_ASSIGN     2
    //#define AFOP_NOR        3
}
