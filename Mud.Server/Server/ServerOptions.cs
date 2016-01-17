using Mud.Server.Blueprints;

namespace Mud.Server.Server
{
    public static class ServerOptions
    {
        public const bool PrefixForwardedMessages = false; // Add <IMP> or <CTRL> before forwarding a message
        public const bool ForwardSlaveMessages = false; // Forward messages received by a slaved character
        public const bool RemovePeriodicAurasInNotInSameRoom = true; // If a NPC has dot/hot from a source in another room, they are removed on next Pulse

        public const int PulsePerSeconds = 4;
        public const int PulseDelay = 1000 / PulsePerSeconds;
        public const int PulseViolence = 3 * PulsePerSeconds; // automatic combat (in pulse per seconds)

        public const bool CheckLoginPassword = false;

        public static ItemCorpseBlueprint CorpseBlueprint { get; set; }
    }
}
