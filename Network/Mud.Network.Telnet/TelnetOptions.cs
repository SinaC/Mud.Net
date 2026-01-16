namespace Mud.Network.Telnet
{
    public class TelnetOptions
    {
        public static string SectionName = "Telnet.Settings";

        public required int Port { get; init; }
    }
}
