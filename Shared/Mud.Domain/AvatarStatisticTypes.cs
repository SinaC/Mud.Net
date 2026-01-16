namespace Mud.Domain;

public enum AvatarStatisticTypes
{
    NpcKill = 0, // number of NPC victims
    PcKill = 1, // number of PC victims
    NpcKilled = 2, // times being killed by NPC
    PcKilled = 3, // times being killed by PC
    NoSourceKilled = 4, // times being killed by ???
    MoneySpent = 5, // money spent in shops (in silver)
    GoldEarned = 6, // gold earned from loot/sell/split/...
    GoldStolen = 7, // gold stolen by other pc/npc
    SilverEarned = 8, // silver earned from loot/sell/split/...
    SilverStolen = 9, // silver stolen by other pc/npc
    RecallUsed = 10, // number of times recall has been used
    FoodEaten = 11, // number of food eaten
    BeveragesConsumed = 12, // number of times drink has been used
    PillEaten = 13, // number of pills eaten
    PotionQuaffed = 14, // number of potions quaffed
    PortalUsed = 15, // number of times drink has been entered
    QuestPointsEarned = 16, // quest points earned
    QuestPointsSpent = 17, // quests points spent
    GeneratedQuestsRequested = 18, // number of times a automatic/generated quest has been requested
    GeneratedQuestsCompleted = 19, // number of times a automatic/generated quest has been complete
    GeneratedQuestsAbandoned = 20, // number of times a automatic/generated quest has been abandoned
    GeneratedQuestsTimedout = 21, // number of times a automatic/generated quest has been reached time out
    PredefinedQuestsRequested = 22, // number of times a predefined quest has been requested
    PredefinedQuestsCompleted = 23, // number of times a predefined quest has been complete
    PredefinedQuestsAbandoned = 24, // number of times a predefined quest has been abandoned
    PredefinedQuestsTimedout = 25, // number of times a predefined quest has been reached time out
}
