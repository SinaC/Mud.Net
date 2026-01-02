using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Actor;
using Mud.Server.Common.Extensions;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Mud.Server.Player;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[Export(typeof(IPlayer))]
public class Player : ActorBase, IPlayer
{
    protected ICommandParser CommandParser { get; }
    protected ITimeManager TimeManager { get; }
    protected ICharacterManager CharacterManager { get; }

    private readonly List<string> _delayedTells;
    private readonly List<PlayableCharacterData> _avatarList;
    private readonly Dictionary<string, string> _aliases;

    private string? _lastInput;
    private string? _lastCommand;
    private DateTime? _lastCommandTimestamp;

    protected IInputTrap<IPlayer>? CurrentStateMachine;

    public Player(ILogger<Player> logger, IGameActionManager gameActionManager, ICommandParser commandParser, ITimeManager timeManager, ICharacterManager characterManager)
        : base(logger, gameActionManager)
    {
        CommandParser = commandParser;
        TimeManager = timeManager;
        CharacterManager = characterManager;

        _delayedTells = [];
        _avatarList = [];
        _aliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }

    protected void Initialize(Guid id, string name)
    {
        Id = id;
        Name = name;

        PlayerState = PlayerStates.Loading;

        CurrentStateMachine = null!;
        DeletionConfirmationNeeded = false;
        PagingLineCount = 24;
    }

    // Used for promote
    public void Initialize(Guid id, string name, IReadOnlyDictionary<string, string> aliases, IEnumerable<PlayableCharacterData> avatarList)
    {
        Initialize(id, name);

        foreach(var alias in aliases)
            _aliases.Add(alias.Key, alias.Value);
        foreach(var avatar in avatarList)
            _avatarList.Add(avatar);
    }

    public void Initialize(Guid id, PlayerData data)
    {
        Initialize(id, data.Name);

        PagingLineCount = data.PagingLineCount;
        _aliases.Clear();
        _avatarList.Clear();
        if (data.Aliases != null)
        {
            foreach (KeyValuePair<string, string> alias in data.Aliases)
                _aliases.Add(alias.Key, alias.Value);
        }

        if (data.Characters != null)
        {
            foreach (PlayableCharacterData playableCharacterData in data.Characters)
                _avatarList.Add(playableCharacterData);
        }
    }

    #region IPlayer

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions
        => GameActionManager.GetGameActions<Player>();

    public override bool ProcessInput(string input)
    {
        // If an input state machine is running, send commandLine to machine
        if (CurrentStateMachine != null && !CurrentStateMachine.IsFinalStateReached)
        {
            CurrentStateMachine.ProcessInput(this, input);
            return true;
        }
        else
        {
            CurrentStateMachine = null; // reset current state machine if not currently running one
            // ! means repeat last command (only when last command was not delete)
            if (input != null && input.Length >= 1 && input[0] == '!')
            {
                if (_lastCommand != null && (StringCompareHelpers.StringEquals(_lastCommand, "delete") || StringCompareHelpers.StringEquals(_lastCommand, "deleteavatar")))
                {
                    Send("Cannot use '!' to repeat 'delete' or 'deleteavatar' command");
                    DeletionConfirmationNeeded = false; // reset delete confirmation
                    AvatarNameDeletionConfirmationNeeded = null; // reset avatar delete confirmation
                    return false;
                }
                input = _lastInput!;
                _lastCommandTimestamp = TimeManager.CurrentTime;
            }
            else
            {
                _lastInput = input;
                _lastCommandTimestamp = TimeManager.CurrentTime;
            }

            // Extract command and parameters
            bool extractedSuccessfully = CommandParser.ExtractCommandAndParameters(
                isForceOutOfGame => isForceOutOfGame || Impersonating == null
                    ? Aliases
                    : Impersonating?.Aliases,
                input,
                out string command, out ICommandParameter[] parameters, out bool forceOutOfGame);
            _lastCommand = command;
            if (!extractedSuccessfully)
            {
                Logger.LogWarning("Command and parameters not extracted successfully");
                Send("Invalid command or parameters.");
                return false;
            }

            // Choose correct context to execute command and execute it (depends on Impersonating, Incarnting, force out of game, ...)
            return ContextWiseExecuteCommand(input!, command, parameters, forceOutOfGame);
        }
    }

    public override void Send(string message, bool addTrailingNewLine)
    {
        if (addTrailingNewLine)
            message = message + Environment.NewLine;
        SendData?.Invoke(this, message);
        if (SnoopBy != null)
        {
            StringBuilder sb = new();
            sb.Append(DisplayName);
            sb.Append("> ");
            sb.Append(message);
            SnoopBy.Send(sb);
        }
    }

    public override void Page(StringBuilder text)
    {
        if (PagingLineCount == 0) // no paging
            Send(text.ToString(), false);
        else
        {
            PageData?.Invoke(this, text);
            if (SnoopBy != null)
            {
                StringBuilder sb = new();
                sb.Append(DisplayName);
                sb.Append("[paged]> ");
                sb.Append(text);
                SnoopBy.Send(sb);
            }
        }
    }

    #endregion

    public event SendDataEventHandler? SendData;
    public event PageDataEventHandler? PageData;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    public string DisplayName => Name.UpperFirstLetter();

    public int PagingLineCount { get; protected set; }

    public void SetPagingLineCount(int count)
    {
        PagingLineCount = count;
    }

    public PlayerStates PlayerState { get; protected set; }

    public void ChangePlayerState(PlayerStates playerState)
    {
        PlayerState = playerState;
    }

    public IPlayableCharacter? Impersonating { get; private set; }

    public void UpdateCharacterDataFromImpersonated()
    {
        if (Impersonating == null)
        {
            Logger.LogError("UpdateCharacterDataFromImpersonated while not impersonated.");
            return;
        }
        int index = _avatarList.FindIndex(x => StringCompareHelpers.StringEquals(x.Name, Impersonating.Name));
        if (index < 0)
        {
            Logger.LogError("UpdateCharacterDataFromImpersonated: unknown avatar {impersonating} for player {playerName}", Impersonating.DebugName, DisplayName);
            return;
        }

        PlayableCharacterData updatedCharacterData = Impersonating.MapPlayableCharacterData();
        _avatarList[index] = updatedCharacterData; // replace with new character data
    }

    public IEnumerable<PlayableCharacterData> Avatars => _avatarList;

    public IReadOnlyDictionary<string, string> Aliases => _aliases;

    public void SetAlias(string alias, string command)
    {
        _aliases[alias] = command;
    }

    public void RemoveAlias(string alias)
    {
        _aliases.Remove(alias);
    }

    public IPlayer? LastTeller { get; private set; }

    public IAdmin? SnoopBy { get; private set; } // every messages send to 'this' will be sent to SnoopBy

    public virtual string Prompt => Impersonating != null
        ? BuildCharacterPrompt(Impersonating)
        : ">";

    // lag
    public int Lag { get; private set; }
    public void DecreaseLag() // decrease one by one
    {
        Lag = Math.Max(Lag - 1, 0);
    }
    public void SetLag(int pulseCount) // set lag delay (in pulse), can only increase
    {
        if (pulseCount > Lag)
            Lag = pulseCount;
    }

    // afk/tell
    public bool IsAfk { get; protected set; }

    public IEnumerable<string> DelayedTells => _delayedTells; // Tell stored while AFK

    public void ToggleAfk()
    {
        if (IsAfk)
        {
            Send("%G%AFK%x% removed.");
            if (DelayedTells.Any())
                Send("%r%You have received tells: Type %Y%'replay'%r% to see them.%x%");
        }
        else
            Send("You are now in %G%AFK%x% mode.");
        IsAfk = !IsAfk;
    }

    public void SetLastTeller(IPlayer? teller)
    {
        LastTeller = teller;
    }

    public void AddDelayedTell(string sentence)
    {
        _delayedTells.Add(sentence);
    }

    public void ClearDelayedTells()
    {
        _delayedTells.Clear();
    }

    public void SetSnoopBy(IAdmin? snooper)
    {
        SnoopBy = snooper;
    }

    public string? AvatarNameDeletionConfirmationNeeded { get; private set; }

    public void SetAvatarNameDeletionConfirmationNeeded(string avatarName)
    {
        AvatarNameDeletionConfirmationNeeded = avatarName;
    }

    public void ResetAvatarNameDeletionConfirmationNeeded()
    {
        AvatarNameDeletionConfirmationNeeded = null;
    }

    public void AddAvatar(PlayableCharacterData playableCharacterData)
    {
        _avatarList.Add(playableCharacterData);
    }

    public bool DeleteAvatar(string avatarName)
    {
        var avatar = _avatarList.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, avatarName));
        if (avatar == null)
            return false;
        _avatarList.Remove(avatar);
        return true;
    }

    public void StartImpersonating(IPlayableCharacter avatar)
    {
        Impersonating = avatar;
        PlayerState = PlayerStates.Impersonating;
        var result = GameActionManager.Execute<Commands.Character.Information.Look, ICharacter>(Impersonating, null);
        if (result != null)
            avatar.Send(result);
    }

    public void StopImpersonating()
    {
        if (Impersonating == null)
        {
            Logger.LogError("StopImpersonating while not impersonated.");
            return;
        }
        Impersonating.StopImpersonation();
        CharacterManager.RemoveCharacter(Impersonating); // extract avatar  TODO: linkdead instead of RemoveCharacter ?
        Impersonating = null;
        PlayerState = PlayerStates.Playing;
    }

    public bool DeletionConfirmationNeeded { get; protected set; }

    public void SetDeletionConfirmationNeeded()
    {
        DeletionConfirmationNeeded = true;
    }

    public void ResetDeletionConfirmationNeeded()
    {
        DeletionConfirmationNeeded = false;
    }

    public bool SaveNeeded { get; protected set; }

    public void SetSaveNeeded()
    {
        SaveNeeded = true;
    }

    public void ResetSaveNeeded()
    {
        SaveNeeded = false;
    }

    public void SetStateMachine(IInputTrap<IPlayer> inputTrap)
    {
        CurrentStateMachine = inputTrap;
    }

    public virtual void OnDisconnected()
    {
        LastTeller?.Send($"{DisplayName} has left the game.");
        LastTeller = null;
        SnoopBy?.Send($"Your victim {DisplayName} has left the game.");
        SnoopBy = null;
        // Stop impersonation if any + stop fights
        if (Impersonating != null)
        {
            Impersonating.StopFighting(true);
            StopImpersonating();
        }
    }

    public virtual PlayerData MapPlayerData()
    {
        if (Impersonating != null)
            UpdateCharacterDataFromImpersonated();
        PlayerData data = new()
        {
            Name = Name,
            PagingLineCount = PagingLineCount,
            Aliases = Aliases.ToDictionary(x => x.Key, x => x.Value),
            Characters = _avatarList.ToArray(),
        };
        return data;
    }

    public virtual StringBuilder PerformSanityChecks()
    {
        StringBuilder sb = new();

        sb.AppendLine("--Player--");
        sb.AppendLine($"Id: {Id}");
        sb.AppendLine($"Name: {Name}");
        sb.AppendLine($"DisplayName: {DisplayName}");
        sb.AppendLine($"_lastInput: {_lastInput} Timestamp: {_lastCommandTimestamp}");
        sb.AppendLine($"IsAfk: {IsAfk}");
        sb.AppendLine($"PlayerState: {PlayerState}");
        sb.AppendLine($"Lag: {Lag}");
        sb.AppendLine($"GCD: {Impersonating?.GlobalCooldown}");
        sb.AppendLine($"DAZE: {Impersonating?.Daze}");
        sb.AppendLine($"Aliases: {Aliases?.Count ?? 0}");
        sb.AppendLine($"Avatars: {_avatarList?.Count ?? 0}");
        sb.AppendLine($"SnoopBy: {SnoopBy?.DisplayName ?? "none"}");
        sb.AppendLine($"LastTeller: {LastTeller?.DisplayName ?? "none"}");
        sb.AppendLine($"DelayedTells: {DelayedTells?.Count() ?? 0}");
        sb.AppendLine($"Impersonating: {Impersonating?.DisplayName ?? "none"}");
        sb.AppendLine($"CurrentStateMachine: {CurrentStateMachine}");

        return sb;
    }

    #endregion

    protected virtual bool ContextWiseExecuteCommand(string commandLine, string command, ICommandParameter[] parameters, bool forceOutOfGame)
    {
        // Execute command
        bool executedSuccessfully;
        if (forceOutOfGame || Impersonating == null)
        {
            Logger.LogDebug("[{name}] executing [{command}]", DisplayName, commandLine);
            executedSuccessfully = ExecuteCommand(commandLine, command, parameters);
        }
        else if (Impersonating != null) // impersonating
        {
            Logger.LogDebug("[{name}]|[{impersonatingName}] executing [{command}]", DisplayName, Impersonating.DebugName, commandLine);
            executedSuccessfully = Impersonating.ExecuteCommand(commandLine, command, parameters);
        }
        else
        {
            Logger.LogError("[{name}] is neither out of game nor impersonating", DisplayName);
            executedSuccessfully = false;
        }
        if (!executedSuccessfully)
            Logger.LogWarning("Error while executing command");
        return executedSuccessfully;
    }

    protected static string BuildCharacterPrompt(IPlayableCharacter character) // TODO: custom prompt defined by player
    {
        StringBuilder sb = new ("%c%<");
        foreach (var resourceKinds in character.CurrentResourceKinds.OrderBy(x => x))
            sb.Append($"{character[resourceKinds]}/{character.MaxResource(resourceKinds)}{resourceKinds.ResourceName()} ");
        sb.Append($"{character.ExperienceToLevel}Nxt");
        if (character.Fighting != null)
            sb.Append($" {(100 * character.Fighting[ResourceKinds.HitPoints])/character.Fighting.MaxResource(ResourceKinds.HitPoints)}%");
        sb.Append(">%x%");
        return sb.ToString();
    }

    //
    private string DebuggerDisplay => $"Player {Name} IMP:{Impersonating?.Name}";
}
