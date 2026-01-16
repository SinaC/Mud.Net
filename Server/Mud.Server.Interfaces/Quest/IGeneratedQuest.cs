using Mud.Blueprints.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Quest;

public interface IGeneratedQuest : IQuest
{
    bool Initialize(IPlayableCharacter character, INonPlayableCharacter giver, int itemQuestBlueprintId, IRoom room, int level, int timeLimit); // find item
    bool Initialize(IPlayableCharacter character, INonPlayableCharacter giver, int itemQuestBlueprintId, INonPlayableCharacter target, IRoom room, int timeLimit); // loot item
    bool Initialize(IPlayableCharacter character, INonPlayableCharacter giver, INonPlayableCharacter target, IRoom room, int timeLimit); // kill mob

    GeneratedQuestType GeneratedQuestType { get; }
    INonPlayableCharacter? Target { get; }
    ItemQuestBlueprint? ItemQuestBlueprint { get; }
    IRoom Room { get; }

    void Delete();
}

public enum GeneratedQuestType
{
    FindItem = 1,
    LootItem = 2,
    KillMob = 3
}
