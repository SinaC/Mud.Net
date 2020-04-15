namespace Mud.Settings
{
    public interface ISettings
    {
        string LogPath { get; }
        string PlayerRepositoryPath { get; }
        string AdminRepositoryPath { get; }
        string LoginRepositoryFilename { get; }
        string ImportAreaPath { get; }

        // Add <IMP> or <CTRL> before forwarding a message
        bool PrefixForwardedMessages { get; }
        // Forward messages received by a slaved character
        bool ForwardSlaveMessages { get; }
        // If a NPC has dot/hot from a source in another room, they are removed on next Pulse
        bool RemovePeriodicAurasInNotInSameRoom { get; }

        int CorpseBlueprintId { get; }

        int PulsePerSeconds { get; }
        int PulsePerMinutes { get; }
        int PulseDelay { get; }
        // automatic combat (in pulse per seconds)
        int PulseViolence { get; }

        bool CheckLoginPassword { get; }

        int IdleMinutesBeforeUnimpersonate { get; }
        int IdleMinutesBeforeDisconnect { get; }

        int MaxLevel { get; }
        int MaxAvatarCount { get; }
    }
}
