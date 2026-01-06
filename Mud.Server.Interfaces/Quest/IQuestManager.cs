using Mud.Blueprints.Quest;
using Mud.Server.Interfaces.Character;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Interfaces.Quest;

public interface IQuestManager
{
    IReadOnlyCollection<QuestBlueprint> QuestBlueprints { get; }

    QuestBlueprint? GetQuestBlueprint(int id);

    void AddQuestBlueprint(QuestBlueprint blueprint);

    IPredefinedQuest? AddQuest(QuestBlueprint questBlueprint, IPlayableCharacter pc, INonPlayableCharacter questGiver);
    IPredefinedQuest? AddQuest(CurrentQuestData questData, IPlayableCharacter pc);
    IGeneratedQuest? GenerateQuest(IPlayableCharacter pc, INonPlayableCharacter questGiver);
}
