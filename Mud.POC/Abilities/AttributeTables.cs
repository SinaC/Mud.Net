using Mud.Common;
using Mud.Server.Random;

namespace Mud.POC.Abilities
{
    public static class AttributeTables
    {
        public static int GetLearnPercentage(ICharacter character)
        {
            int intelligence = character.CurrentAttributes(CharacterAttributes.Intelligence);
            int index = intelligence.Range(LearnPercentageByIntelligenceTable);
            return LearnPercentageByIntelligenceTable[index];
        }

        private static readonly int[] LearnPercentageByIntelligenceTable = {
            3, //  0
            5, //  1
            7,
            8, //  3
            9,
            10, // 5
            11,
            12,
            13,
            15,
            17, // 10
            19,
            22,
            25,
            28,
            31, // 15
            34,
            37,
            40, // 18
            44,
            49, // 20
            55,
            60,
            70,
            80,
            85 //  25
    };
}
}
