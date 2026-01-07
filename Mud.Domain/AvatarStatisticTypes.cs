namespace Mud.Domain;

public enum AvatarStatisticTypes
{
    NpcKill = 0, // number of NPC victims
    PcKill = 1, // number of PC victims
    NpcKilled = 2, // times being killed by NPC
    PcKilled = 3, // times being killed by PC
    NoSourceKilled = 4, // times being killed by ???
    GoldEarned = 5, // gold earned from loot/sell/split/...
    GoldSpent = 6, // gold spent in shops
    GoldStolen = 7, // gold stolen by other pc/npc
    SilverEarned = 8, // silver earned from loot/sell/split/...
    SilverSpent = 9, // silver spent in shops
    SilverStolen = 10, // silver stolen by other pc/npc
    RecallUsed = 11, // number of times recall has been used
    FoodEaten = 12, // number of food eaten
    BeveragesConsumed = 13, // number of times drink has been used
    PillEaten = 14, // number of pills eaten
    PotionQuaffed = 15, // number of potions quaffed
    PortalUsed = 16, // number of times drink has been entered
    QuestPointsEarned = 17, // quest points earned
    QuestPointsSpent = 18, // quests points spent
    GeneratedQuestsRequested = 19, // number of times a automatic/generated quest has been requested
    GeneratedQuestsCompleted = 20, // number of times a automatic/generated quest has been complete
    GeneratedQuestsAbandoned = 21, // number of times a automatic/generated quest has been abandoned
    GeneratedQuestsTimedout = 22, // number of times a automatic/generated quest has been reached time out
    PredefinedQuestsRequested = 23, // number of times a predefined quest has been requested
    PredefinedQuestsCompleted = 24, // number of times a predefined quest has been complete
    PredefinedQuestsAbandoned = 25, // number of times a predefined quest has been abandoned
    PredefinedQuestsTimedout = 26, // number of times a predefined quest has been reached time out
}
