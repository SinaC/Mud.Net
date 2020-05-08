using System.Linq;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;

namespace Mud.Server.Tables
{
    public class TableValues : ITableValues
    {
        public (int hit, int dam, int carry, int wield, int learn, int practice, int defensive, int hitpoint, int shock) Bonus(ICharacter character)
        {
            (int hit, int dam, int carry, int wield) = StrengthBasedBonus.Get(character[CharacterAttributes.Strength]);
            int learn = IntelligenceBasedBonus.Get(character[CharacterAttributes.Intelligence]);
            int practice = WisdomBasedBonus.Get(character[CharacterAttributes.Wisdom]);
            int defensive = DexterityBasedBonus.Get(character[CharacterAttributes.Dexterity]);
            (int hitpoint, int shock) = ConstitutionBasedBonus.Get(character[CharacterAttributes.Constitution]);

            return (hit, dam, carry, wield, learn, practice, defensive, hitpoint, shock);
        }

        public int HitBonus(ICharacter character) => StrengthBasedBonus.Get(character[CharacterAttributes.Strength]).hit;

        public int DamBonus(ICharacter character) => StrengthBasedBonus.Get(character[CharacterAttributes.Strength]).dam;

        public int CarryBonus(ICharacter character) => StrengthBasedBonus.Get(character[CharacterAttributes.Strength]).carry;

        public int WieldBonus(ICharacter character) => StrengthBasedBonus.Get(character[CharacterAttributes.Strength]).wield;

        public int LearnBonus(ICharacter character) => IntelligenceBasedBonus.Get(character[CharacterAttributes.Intelligence]);

        public int PracticeBonus(ICharacter character) => WisdomBasedBonus.Get(character[CharacterAttributes.Wisdom]);

        public int DefensiveBonus(ICharacter character) => DexterityBasedBonus.Get(character[CharacterAttributes.Dexterity]);

        public int HitpointBonus(ICharacter character) => ConstitutionBasedBonus.Get(character[CharacterAttributes.Constitution]).hitpoint;

        public int ShockBonus(ICharacter character) => ConstitutionBasedBonus.Get(character[CharacterAttributes.Constitution]).shock;

        public int EquipmentSlotMultiplier(EquipmentSlots slot)
        {
            switch (slot)
            {
                case EquipmentSlots.None: return 1;
                case EquipmentSlots.Light: return 1;
                case EquipmentSlots.Head: return 2;
                case EquipmentSlots.Amulet: return 1;
                case EquipmentSlots.Shoulders: return 1;
                case EquipmentSlots.Chest: return 3;
                case EquipmentSlots.Cloak: return 2;
                case EquipmentSlots.Waist: return 1;
                case EquipmentSlots.Wrists: return 1;
                case EquipmentSlots.Arms: return 1;
                case EquipmentSlots.Hands: return 1;
                case EquipmentSlots.Ring: return 1;
                case EquipmentSlots.Legs: return 2;
                case EquipmentSlots.Feet: return 1;
                case EquipmentSlots.Trinket: return 1;
                case EquipmentSlots.MainHand: return 1;
                case EquipmentSlots.OffHand: return 1;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Invalid EquipmentSlots {0}", slot);
                    return 1;
            }
        }

        public (string name, string color, int proof, int full, int thirst, int food, int servingsize) LiquidInfo(string liquidName) => LiquidTable.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.name, liquidName));

        private static readonly (int hit, int dam, int carry, int wield)[] StrengthBasedBonus = {
            ( -5, -4,   0,  0 ),  /* 0  */
            ( -5, -4,   3,  1 ),  /* 1  */
            ( -3, -2,   3,  2 ),
            ( -3, -1,  10,  3 ),  /* 3  */
            ( -2, -1,  25,  4 ),
            ( -2, -1,  55,  5 ),  /* 5  */
            ( -1,  0,  80,  6 ),
            ( -1,  0,  90,  7 ),
            (  0,  0, 100,  8 ),
            (  0,  0, 100,  9 ),
            (  0,  0, 115, 10 ), /* 10  */
            (  0,  0, 115, 11 ),
            (  0,  0, 130, 12 ),
            (  0,  0, 130, 13 ), /* 13  */
            (  0,  1, 140, 14 ),
            (  1,  1, 150, 15 ), /* 15  */
            (  1,  2, 165, 16 ),
            (  2,  3, 180, 22 ),
            (  2,  3, 200, 25 ), /* 18  */
            (  3,  4, 225, 30 ),
            (  3,  5, 250, 35 ), /* 20  */
            (  4,  6, 300, 40 ),
            (  4,  6, 350, 45 ),
            (  5,  7, 400, 50 ),
            (  5,  8, 450, 55 ),
            (  6,  9, 500, 60 )  /* 25   */
        };

        private static readonly int[] IntelligenceBasedBonus = {
            3,  /*  0 */
            5,  /*  1 */
            7,
            8,  /*  3 */
            9,
            10, /*  5 */
            11,
            12,
            13,
            15,
            17, /* 10 */
            19,
            22,
            25,
            28,
            31, /* 15 */
            34,
            37,
            40, /* 18 */
            44,
            49, /* 20 */
            55,
            60,
            70,
            80,
            85  /* 25 */
        };

        private static readonly int[] WisdomBasedBonus = {
             0 ,	/*  0 */
             0 ,	/*  1 */
             0 ,
             0 ,	/*  3 */
             0 ,
             1 ,	/*  5 */
             1 ,
             1 ,
             1 ,
             1 ,
             1 ,	/* 10 */
             1 ,
             1 ,
             1 ,
             1 ,
             2 ,	/* 15 */
             2 ,
             2 ,
             3 ,	/* 18 */
             3 ,
             3 ,	/* 20 */
             3 ,
             4 ,
             4 ,
             4 ,
             5 	/* 25 */
        };

        private static readonly int[] DexterityBasedBonus = {
            60 ,   /* 0 */
            50 ,   /* 1 */
            50 ,
            40 ,
            30 ,
            20 ,   /* 5 */
            10 ,
            0 ,
            0 ,
            0 ,
            0 ,   /* 10 */
            0 ,
            0 ,
            0 ,
            0 ,
            - 10 ,   /* 15 */
            - 15 ,
            - 20 ,
            - 30 ,
            - 40 ,
            - 50 ,   /* 20 */
            - 60 ,
            - 75 ,
            - 90 ,
            -105 ,
            -120     /* 25 */
        };

        private static readonly (int hitpoint, int shock)[] ConstitutionBasedBonus = {
            ( -4, 20 ),   /*  0 */
            ( -3, 25 ),   /*  1 */
            ( -2, 30 ),
            ( -2, 35 ),	  /*  3 */
            ( -1, 40 ),
            ( -1, 45 ),   /*  5 */
            ( -1, 50 ),
            (  0, 55 ),
            (  0, 60 ),
            (  0, 65 ),
            (  0, 70 ),   /* 10 */
            (  0, 75 ),
            (  0, 80 ),
            (  0, 85 ),
            (  0, 88 ),
            (  1, 90 ),   /* 15 */
            (  2, 95 ),
            (  2, 97 ),
            (  3, 99 ),   /* 18 */
            (  3, 99 ),
            (  4, 99 ),   /* 20 */
            (  4, 99 ),
            (  5, 99 ),
            (  6, 99 ),
            (  7, 99 ),
            (  8, 99 )    /* 25 */
        };

        private static readonly (string name, string color, int proof, int full, int thirst, int food, int servingsize)[] LiquidTable =
        {
            // 0
            ( "water",             "clear",            0, 1, 10, 0, 16 ),
            ( "beer",              "amber",           12, 1,  8, 1, 12 ),
            ( "red wine",          "burgundy",        30, 1,  8, 1,  5 ),
            ( "ale",               "brown",           15, 1,  8, 1, 12 ),
            ( "dark ale",          "dark",            16, 1,  8, 1, 12 ),
            // 5                   
            ( "whisky",            "golden",         120, 1,  5, 0,  2 ),
            ( "lemonade",          "pink",             0, 1,  9, 2, 12 ),
            ( "firebreather",      "boiling",        190, 0,  4, 0,  2 ),
            ( "local specialty",   "clear",          151, 1,  3, 0,  2 ),
            ( "slime mold juice",  "green",            0, 2, -8, 1,  2 ),
            // 10
            ( "milk",               "white",           0, 2,  9, 3, 12 ),
            ( "tea",                "tan",             0, 1,  8, 0,  6 ),
            ( "coffee",             "black",           0, 1,  8, 0,  6 ),
            ( "blood",              "red",             0, 2, -1, 2,  6 ),
            ( "salt water",         "clear",           0, 1, -2, 0,  1 ),
            // 15 
            ( "coke",               "brown",           0, 2,  9, 2, 12 ),
            ( "root beer",          "brown",           0, 2,  9, 2, 12 ),
            ( "elvish wine",        "green",          35, 2,  8, 1,  5 ) ,
            ( "white wine",         "golden",         28, 1,  8, 1,  5 ),
            ( "champagne",          "golden",         32, 1,  8, 1,  5 ),
            // 20
            ( "mead",               "honey-colored",  34, 2,  8, 2, 12 ),
            ( "rose wine",          "pink",           26, 1,  8, 1,  5 ),
            ( "benedictine wine",   "burgundy",       40, 1,  8, 1,  5 ),
            ( "vodka",              "clear",         130, 1,  5, 0,  2 ),
            ( "cranberry juice",    "red",             0, 1,  9, 2, 12  ),
            // 25
            ( "orange juice",       "orange",          0, 2,  9, 3, 12 ),
            ( "absinthe",           "green",         200, 1,  4, 0,  2 ),
            ( "brandy",             "golden",         80, 1,  5, 0,  4 ),
            ( "aquavit",            "clear",         140, 1,  5, 0,  2 ),
            ( "schnapps",           "clear",          90, 1,  5, 0,  2 ),
            // 30
            ( "icewine",            "purple",         50, 2,  6, 1,  5 ),
            ( "amontillado",        "burgundy",       35, 2,  8, 1,  5 ),
            ( "sherry",             "red",            38, 2,  7, 1,  5 ),
            ( "framboise",          "red",            50, 1,  7, 1,  5 ),
            ( "rum",                "amber",         151, 1,  4, 0,  2 ),
            // 31                                 
            ( "cordial",            "clear",         100, 1,  5, 0,  2 ),
        };
    }
}
