using Mud.Blueprints.Item;

namespace Mud.Server.Quest.Objectives;

public abstract class ItemQuestObjectiveBase : QuestObjectiveCountBase
{
    public required ItemQuestBlueprint ItemBlueprint { get; set; }

    #region QuestObjectiveCountBase

    public override string TargetName => ItemBlueprint.ShortDescription;

    #endregion
}
