using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Quest;

public interface IQuestManager
{
    IReadOnlyCollection<QuestBlueprint> QuestBlueprints { get; }

    QuestBlueprint? GetQuestBlueprint(int id);

    void AddQuestBlueprint(QuestBlueprint blueprint);

    IQuest AddQuest(QuestBlueprint questBlueprint, IPlayableCharacter pc, INonPlayableCharacter questGiver);
    IQuest AddQuest(CurrentQuestData questData, IPlayableCharacter pc);
}
