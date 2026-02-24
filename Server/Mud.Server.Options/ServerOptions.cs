namespace Mud.Server.Options
{
    public class ServerOptions
    {
        public const string SectionName = "Server.Settings";

        public required bool DumpOnInitialize { get; init; }
        public required int IdleMinutesBeforeUnimpersonate { get; init; }
        public required int IdleMinutesBeforeDisconnect { get; init; }
        public required bool CheckPassword { get; init; }
        public required string[] ForbiddenNames { get; init; }
        public required int MaxProgramDepth { get; set; }
    }
}
