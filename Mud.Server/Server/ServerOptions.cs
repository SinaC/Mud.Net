using Mud.Server.Blueprints.Item;

namespace Mud.Server.Server
{
    public static class ServerOptions
    {
        public const bool PrefixForwardedMessages = false; // Add <IMP> or <CTRL> before forwarding a message
        public const bool ForwardSlaveMessages = false; // Forward messages received by a slaved character
        public const bool RemovePeriodicAurasInNotInSameRoom = true; // If a NPC has dot/hot from a source in another room, they are removed on next Pulse

        public const int PulsePerSeconds = 4;
        public const int PulsePerMinutes = PulsePerSeconds*60;
        public const int PulseDelay = 1000 / PulsePerSeconds;
        public const int PulseViolence = 3 * PulsePerSeconds; // automatic combat (in pulse per seconds)

        public const bool CheckLoginPassword = false;

        public const int IdleMinutesBeforeUnimpersonate = 10;
        public const int IdleMinutesBeforeDisconnect = 20;

        public const int MaxLevel = 60;
        public const int MaxAvatarCount = 10;

        public const string PagingInstructions = "[Paging : (Enter), (N)ext, (P)revious, (Q)uit, (A)ll]";

        public static ItemCorpseBlueprint CorpseBlueprint { get; set; }
    }
}
