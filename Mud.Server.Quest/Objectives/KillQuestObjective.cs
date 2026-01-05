using Mud.Blueprints.Character;

namespace Mud.Server.Quest.Objectives;

public class KillQuestObjective : QuestObjectiveCountBase
{
    public required CharacterBlueprintBase Blueprint { get; set; }

    #region QuestObjectiveCountBase

    public override string TargetName => Blueprint.ShortDescription;

    #endregion
}
