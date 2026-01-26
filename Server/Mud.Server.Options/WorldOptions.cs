namespace Mud.Server.Options
{
    public class WorldOptions
    {
        public const string SectionName = "World.Settings";

        public required int MaxLevel { get; init; }
        public required bool UseAggro { get; set; }
        public required BlueprintIds BlueprintIds { get; init; }
    }

    public class BlueprintIds
    {
        public required int NullRoom { get; init; }
        public required int Coins { get; init; }
        public required int Corpse { get; init; }
        public required int DefaultRoom { get; init; }
        public required int DefaultRecallRoom { get; init; }
        public required int DefaultDeathRoom { get; init; }
        public required int MudSchoolRoom { get; init; }
    }
}
