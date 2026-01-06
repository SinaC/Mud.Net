using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Interfaces.Player;

public enum PlayerStates
{
    Loading,
    Playing, // if not impersonating and not creating avatar
    CreatingAvatar, // creating avatar
    Impersonating, // playing avatar
}

public delegate void SendDataEventHandler(IPlayer player, string data);
public delegate void PageDataEventHandler(IPlayer player, StringBuilder data);

public interface IPlayer : IActor
{
    void Initialize(Guid id, string name, string password, IReadOnlyDictionary<string, string> aliases, IEnumerable<AvatarMetaData> avatarMetaDatas); // Used for promote
    void Initialize(Guid id, AccountData data);

    event SendDataEventHandler SendData;
    event PageDataEventHandler PageData;

    Guid Id { get; }
    string Name { get; }
    string DisplayName { get; } // First letter is in upper-case

    string Password { get; }
    void ChangePassword(string password);

    int PagingLineCount { get; }
    void SetPagingLineCount(int count);

    PlayerStates PlayerState { get; }
    void ChangePlayerState(PlayerStates playerState);

    IPlayableCharacter? Impersonating { get; }
    void UpdateAvatarMetaDataFromImpersonated();

    IEnumerable<AvatarMetaData> AvatarMetaDatas { get; }

    IReadOnlyDictionary<string, string> Aliases { get; }
    void SetAlias(string alias, string command);
    void RemoveAlias(string alias);

    IPlayer? LastTeller { get; }

    IAdmin? SnoopBy { get; } // every messages send to 'this' will be sent to SnoopBy

    string Prompt { get; }

    // lag
    int Lag { get; }
    void DecreaseLag(); // decrease one by one
    void SetLag(int pulseCount); // set lag delay (in pulse), can only increase

    // afk + tell
    bool IsAfk { get; }
    IEnumerable<string> DelayedTells { get; } // Tell stored while AFK
    void ToggleAfk();

    void SetLastTeller(IPlayer? teller);
    void AddDelayedTell(string sentence);
    void ClearDelayedTells();

    void SetSnoopBy(IAdmin? snooper);

    string? AvatarNameDeletionConfirmationNeeded { get; }
    void SetAvatarNameDeletionConfirmationNeeded(string avatarName);
    void ResetAvatarNameDeletionConfirmationNeeded();
    void AddAvatar(AvatarMetaData avatarMetaData);
    bool DeleteAvatar(string avatarName);
    void StartImpersonating(IPlayableCharacter avatar);
    void StopImpersonating();

    bool DeletionConfirmationNeeded { get; }
    void SetDeletionConfirmationNeeded();
    void ResetDeletionConfirmationNeeded();

    bool SaveNeeded { get; }
    void SetSaveNeeded();
    void ResetSaveNeeded();

    void SetStateMachine(IInputTrap<IPlayer> inputTrap);

    void OnDisconnected();

    StringBuilder PerformSanityChecks();

    AccountData MapAccountData();
}
