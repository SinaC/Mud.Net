namespace Mud.Server.Options
{
    public class MessageForwardOptions
    {
        public const string SectionName = "MessageForward.Settings";

        public required bool PrefixForwardedMessages { get; init; }
        public required bool ForwardSlaveMessages { get; init; }
    }
}
