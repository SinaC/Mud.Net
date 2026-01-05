namespace Mud.Server.Options;

public class QuestOptions
{
    public const string SectionName = "Quest.Settings";

    public required int[] ItemToFindBlueprintIds { get; set; }
}
