using Mud.Blueprints.Quest;

namespace Mud.Blueprints.Character;

public class CharacterQuestorBlueprint : CharacterBlueprintBase
{
    public QuestBlueprint[] QuestBlueprints { get; set; } = default!;
}
