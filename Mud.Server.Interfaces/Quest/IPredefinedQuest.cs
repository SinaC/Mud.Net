using Mud.Blueprints.Quest;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Quest;

public interface IPredefinedQuest : IQuest
{
    void Initialize(QuestBlueprint blueprint, IPlayableCharacter character, INonPlayableCharacter giver); // TODO: giver should be ICharacterQuestor
    bool Initialize(QuestBlueprint blueprint, CurrentQuestData questData, IPlayableCharacter character);

    QuestBlueprint Blueprint { get; }

    CurrentQuestData MapQuestData();

}
