namespace Mud.Server.Rom24
{
    public class Rom24Options
    {
        public const string SectionName = "Rom24.Settings";

        public required BlueprintIds BlueprintIds { get; init; }
    }

    public class BlueprintIds
    {
        public required int Mushroom { get; init; }
        public required int LightBall { get; init; }
        public required int Spring { get; init; }
        public required int FloatingDisc { get; init; }
        public required int Portal { get; init; }
        public required int Rose { get; init; }

    }
}
