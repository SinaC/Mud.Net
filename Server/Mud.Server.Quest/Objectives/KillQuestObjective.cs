using Mud.Blueprints.Character;

namespace Mud.Server.Quest.Objectives;

public class KillQuestObjective : QuestObjectiveCountBase
{
    public required CharacterBlueprintBase TargetBlueprint { get; set; }

    #region QuestObjectiveCountBase

    public override string TargetName => TargetBlueprint.ShortDescription;

    #endregion
}
