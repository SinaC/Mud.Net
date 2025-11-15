using Mud.Domain;
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
    event SendDataEventHandler SendData;
    event PageDataEventHandler PageData;

    Guid Id { get; }
    string Name { get; }
    string DisplayName { get; } // First letter is in upper-case

    int GlobalCooldown { get; } // delay (in Pulse) before next action    check WAIT_STATE

    int PagingLineCount { get; }
    void SetPagingLineCount(int count);

    PlayerStates PlayerState { get; }
    void ChangePlayerState(PlayerStates playerState);

    IPlayableCharacter? Impersonating { get; }
    void UpdateCharacterDataFromImpersonated();

    IEnumerable<PlayableCharacterData> Avatars { get; }

    IReadOnlyDictionary<string, string> Aliases { get; }
    void SetAlias(string alias, string command);
    void RemoveAlias(string alias);

    IPlayer? LastTeller { get; }

    IAdmin? SnoopBy { get; } // every messages send to 'this' will be sent to SnoopBy

    string Prompt { get; }

    bool IsAfk { get; }
    IEnumerable<string> DelayedTells { get; } // Tell stored while AFK
    void ToggleAfk();

    void DecreaseGlobalCooldown(); // decrease one by one
    void SetGlobalCooldown(int pulseCount); // set global cooldown delay (in pulse)

    void SetLastTeller(IPlayer? teller);
    void AddDelayedTell(string sentence);
    void ClearDelayedTells();

    void SetSnoopBy(IAdmin? snooper);

    void AddAvatar(PlayableCharacterData playableCharacterData);
    void StartImpersonating(IPlayableCharacter avatar);
    void StopImpersonating();

    bool DeletionConfirmationNeeded { get; }
    void SetDeletionConfirmationNeeded();
    void ResetDeletionConfirmationNeeded();

    void SetStateMachine(IInputTrap<IPlayer> inputTrap);

    void OnDisconnected();

    StringBuilder PerformSanityCheck();

    PlayerData MapPlayerData();
}
