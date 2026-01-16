using Mud.Blueprints.Quest;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Quest;

public interface IPredefinedQuest : IQuest
{
    QuestBlueprint Blueprint { get; }

    void Initialize(QuestBlueprint blueprint, IPlayableCharacter character, INonPlayableCharacter giver); // TODO: giver should be ICharacterQuestor
    bool Initialize(QuestBlueprint blueprint, ActiveQuestData questData, IPlayableCharacter character);

    void SpawnQuestItemOnFloorIfNeeded();

    ICompletedQuest? GenerateCompletedQuest();

    ActiveQuestData MapQuestData();
}
