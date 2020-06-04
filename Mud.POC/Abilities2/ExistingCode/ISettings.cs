namespace Mud.POC.Abilities2.ExistingCode
{
    public interface ISettings
    {
        string LogPath { get; }

        int TelnetPort { get; }

        bool UseMongo { get; }
        string ConnectionString { get; }

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

        bool PerformSanityCheck { get; }
        bool DumpOnInitialize { get; }

        int CoinsBlueprintId { get; }
        int CorpseBlueprintId { get; }
        int MushroomBlueprintId { get; }
        int LightBallBlueprintId { get; }
        int SpringBlueprintId { get; }
        int PortalBlueprintId { get; }
        int RoseBlueprintId { get; }
        int FloatingDiscBlueprintId { get; }
        int DefaultRoomId { get; }
        int DefaultRecallRoomId { get; }
        int NullRoomId { get; }
        int DefaultDeathRoomId { get; }

        bool CheckLoginPassword { get; }

        int IdleMinutesBeforeUnimpersonate { get; }
        int IdleMinutesBeforeDisconnect { get; }

        int MaxLevel { get; }
        int MaxAvatarCount { get; }
    }
}
