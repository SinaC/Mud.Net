using Mud.Blueprints.Item;

namespace Mud.Server.Quest.Objectives;

public class ItemQuestObjective : QuestObjectiveCountBase
{
    public required ItemQuestBlueprint Blueprint { get; set; }

    #region QuestObjectiveCountBase

    public override string TargetName => Blueprint.ShortDescription;

    #endregion
}
