namespace Mud.Server.Options
{
    public class AvatarOptions
    {
        public const string SectionName = "Avatar.Settings";

        public required int MaxCount { get; init; }
    }
}
