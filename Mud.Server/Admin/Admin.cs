using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Text;

namespace Mud.Server.Admin;

public class Admin : Player.Player, IAdmin
{
    public Admin(IGameActionManager gameActionManager, ITimeManager timeManager, ICharacterManager characterManager, Guid id, string name) 
        : base(gameActionManager, timeManager, characterManager, id, name)
    {
    }

    // used for promotion
    public Admin(IGameActionManager gameActionManager, ITimeManager timeManager, ICharacterManager characterManager, Guid id, string name, AdminLevels level, IReadOnlyDictionary<string,string> aliases, IEnumerable<PlayableCharacterData> avatarList)
        : base(gameActionManager, timeManager, characterManager, id, name, aliases, avatarList)
    {
        Level = level;
    }

    public Admin(IGameActionManager gameActionManager, ITimeManager timeManager, ICharacterManager characterManager, Guid id, AdminData data)
        : base(gameActionManager, timeManager, characterManager, id, data)
    {
        Level = data.AdminLevel;
        WiznetFlags = data.WiznetFlags;
    }

    #region IAdmin

    #region IPlayer

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<Admin>();

    #endregion

    public override string Prompt => Incarnating != null
        ? BuildIncarnatePrompt()
        : base.Prompt;

    public override void OnDisconnected()
    {
        base.OnDisconnected();

        // Stop incarnation if any
        if (Incarnating != null)
        {
            StopIncarnating();
        }
    }

    public override PlayerData MapPlayerData()
    {
        if (Impersonating != null)
            UpdateCharacterDataFromImpersonated();
        //
        AdminData data = new()
        {
            Name = Name,
            PagingLineCount = PagingLineCount,
            Aliases = Aliases.ToDictionary(x => x.Key, x => x.Value),
            Characters = Avatars.ToArray(),
            //
            AdminLevel = Level,
            WiznetFlags = WiznetFlags,
        };
        //
        return data;
    }

    public override StringBuilder PerformSanityCheck()
    {
        var sb = base.PerformSanityCheck();

        sb.AppendLine("--Admin--");
        sb.AppendLine($"Incarnating: {Incarnating?.DebugName ?? "none"}");
        sb.AppendLine($"Level: {Level}");
        sb.AppendLine($"WiznetFlags: {WiznetFlags}");

        return sb;
    }

    #endregion

    public AdminLevels Level { get; }

    public WiznetFlags WiznetFlags { get; private set; }

    public void AddWiznet(WiznetFlags wiznetFlags)
    {
        WiznetFlags |= wiznetFlags;
    }

    public void RemoveWiznet(WiznetFlags wiznetFlags)
    {
        WiznetFlags &= ~wiznetFlags;
    }

    public IEntity? Incarnating { get; private set; }

    public bool StartIncarnating(IEntity entity)
    {
        bool incarnated = entity.ChangeIncarnation(this);
        if (incarnated)
        {
            Incarnating = entity;
            PlayerState = PlayerStates.Impersonating;
        }
        return incarnated;
    }

    public void StopIncarnating()
    {
        Incarnating?.ChangeIncarnation(null);
        Incarnating = null;
    }

    #endregion

    protected override bool ContextWiseExecuteCommand(string commandLine, string command, ICommandParameter[] parameters, bool forceOutOfGame)
    {
        // Execute command
        bool executedSuccessfully;
        if (forceOutOfGame || (Impersonating == null && Incarnating == null))
        {
            Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DisplayName, commandLine);
            executedSuccessfully = ExecuteCommand(commandLine, command, parameters);
        }
        else if (Impersonating != null) // impersonating
        {
            Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Impersonating.DebugName, commandLine);
            executedSuccessfully = Impersonating.ExecuteCommand(commandLine, command, parameters);
        }
        else if (Incarnating != null) // incarnating
        {
            Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Incarnating.DebugName, commandLine);
            executedSuccessfully = Incarnating.ExecuteCommand(commandLine, command, parameters);
        }
        else
        {
            Log.Default.WriteLine(LogLevels.Error, "[{0}] is neither out of game nor impersonating nor incarnating", DisplayName);
            executedSuccessfully = false;
        }
        if (!executedSuccessfully)
            Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
        return executedSuccessfully;
    }

    private string BuildIncarnatePrompt()
    {
        if (Incarnating is IPlayableCharacter playableCharacter)
            return BuildCharacterPrompt(playableCharacter);
        return $"{Incarnating?.DebugName}>";
    }
}
