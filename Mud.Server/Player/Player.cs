﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Actor;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Admin;
using Mud.Common;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.GameAction;

namespace Mud.Server.Player
{
    public class Player : ActorBase, IPlayer
    {
        protected ITimeManager TimeManager => DependencyContainer.Current.GetInstance<ITimeManager>();
        protected ICharacterManager CharacterManager => DependencyContainer.Current.GetInstance<ICharacterManager>();

        private readonly List<string> _delayedTells;
        private readonly List<PlayableCharacterData> _avatarList;
        private readonly Dictionary<string, string> _aliases;

        private string _lastCommand;
        private DateTime _lastCommandTimestamp;

        protected IInputTrap<IPlayer> CurrentStateMachine;

        protected Player()
        {
            PlayerState = PlayerStates.Loading;

            _delayedTells = new List<string>();
            _avatarList = new List<PlayableCharacterData>();
            _aliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            CurrentStateMachine = null;
            DeletionConfirmationNeeded = false;
            PagingLineCount = 24;
        }

        public Player(Guid id, string name)
            : this()
        {
            Id = id;
            Name = name;
        }

        // Used for promote
        public Player(Guid id, string name, IReadOnlyDictionary<string, string> aliases, IEnumerable<PlayableCharacterData> avatarList)
            : this(id, name)
        {
            _aliases = aliases?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            _avatarList = avatarList?.ToList() ?? new List<PlayableCharacterData>();
        }

        public Player(Guid id, PlayerData data)
            : this(id, data.Name)
        {
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

        public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<Player>();

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
                    if (_lastCommand?.ToLowerInvariant() == "delete")
                    {
                        Send("Cannot use '!' to repeat 'delete' command");
                        DeletionConfirmationNeeded = false; // reset delete confirmation
                        return false;
                    }
                    input = _lastCommand;
                    _lastCommandTimestamp = TimeManager.CurrentTime;
                }
                else
                {
                    _lastCommand = input;
                    _lastCommandTimestamp = TimeManager.CurrentTime;
                }

                // Extract command and parameters
                bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(
                    isForceOutOfGame => isForceOutOfGame || Impersonating == null
                        ? Aliases
                        : Impersonating?.Aliases,
                    input,
                    out string command, out ICommandParameter[] parameters, out bool forceOutOfGame);
                if (!extractedSuccessfully)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                    Send("Invalid command or parameters.");
                    return false;
                }

                // Choose correct context to execute command and execute it (depends on Impersonating, Incarnting, force out of game, ...)
                return ContextWiseExecuteCommand(input, command, parameters, forceOutOfGame);
            }
        }

        public override void Send(string message, bool addTrailingNewLine)
        {
            if (addTrailingNewLine)
                message = message + Environment.NewLine;
            SendData?.Invoke(this, message);
            if (SnoopBy != null)
            {
                StringBuilder sb = new StringBuilder();
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
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DisplayName);
                    sb.Append("[paged]> ");
                    sb.Append(text);
                    SnoopBy.Send(sb);
                }
            }
        }

        #endregion

        public event SendDataEventHandler SendData;
        public event PageDataEventHandler PageData;

        public Guid Id { get; }
        public string Name { get; }

        public string DisplayName => Name.UpperFirstLetter();

        public int GlobalCooldown { get; protected set; } // delay (in Pulse) before next action

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

        public IPlayableCharacter Impersonating { get; private set; }

        public void UpdateCharacterDataFromImpersonated()
        {
            if (Impersonating == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "UpdateCharacterDataFromImpersonated while not impersonated.");
                return;
            }
            int index = _avatarList.FindIndex(x => StringCompareHelpers.StringEquals(x.Name, Impersonating.Name));
            if (index < 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "UpdateCharacterDataFromImpersonated: unknown avatar {0} for player {1}", Impersonating.DebugName, DisplayName);
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

        public IPlayer LastTeller { get; private set; }

        public IAdmin SnoopBy { get; private set; } // every messages send to 'this' will be sent to SnoopBy

        public virtual string Prompt => Impersonating != null
            ? BuildCharacterPrompt(Impersonating)
            : ">";

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

        public void DecreaseGlobalCooldown() // decrease one by one
        {
            GlobalCooldown = Math.Max(GlobalCooldown - 1, 0);
        }

        public void SetGlobalCooldown(int pulseCount) // set global cooldown delay (in pulse)
        {
            GlobalCooldown = pulseCount;
        }

        public void SetLastTeller(IPlayer teller)
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

        public void SetSnoopBy(IAdmin snooper)
        {
            SnoopBy = snooper;
        }

        public void AddAvatar(PlayableCharacterData playableCharacterData)
        {
            _avatarList.Add(playableCharacterData);
        }

        public void StartImpersonating(IPlayableCharacter avatar)
        {
            Impersonating = avatar;
            PlayerState = PlayerStates.Impersonating;
            StringBuilder sb = new StringBuilder();
            avatar.Room.Append(sb, avatar);
            avatar.Send(sb);
        }

        public void StopImpersonating()
        {
            Impersonating?.StopImpersonation();
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

        public void SetStateMachine(IInputTrap<IPlayer> inputTrap)
        {
            CurrentStateMachine = inputTrap;
        }

        public virtual void OnDisconnected()
        {
            LastTeller = null;
            LastTeller?.Send($"{DisplayName} has left the game.");
            SnoopBy?.Send($"Your victim {DisplayName} has left the game.");
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
            PlayerData data = new PlayerData
            {
                Name = Name,
                PagingLineCount = PagingLineCount,
                Aliases = Aliases.ToDictionary(x => x.Key, x => x.Value),
                Characters = _avatarList.ToArray(),
            };
            return data;
        }

        public virtual StringBuilder PerformSanityCheck()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("--Player--");
            sb.AppendLine($"Id: {Id}");
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"DisplayName: {DisplayName}");
            sb.AppendLine($"_lastCommand: {_lastCommand} Timestamp: {_lastCommandTimestamp}");
            sb.AppendLine($"IsAfk: {IsAfk}");
            sb.AppendLine($"PlayerState: {PlayerState}");
            sb.AppendLine($"GCD: {GlobalCooldown}");
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
                Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DisplayName, commandLine);
                executedSuccessfully = ExecuteCommand(commandLine, command, parameters);
            }
            else if (Impersonating != null) // impersonating
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Impersonating.DebugName, commandLine);
                executedSuccessfully = Impersonating.ExecuteCommand(commandLine, command, parameters);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Error, "[{0}] is neither out of game nor impersonating", DisplayName);
                executedSuccessfully = false;
            }
            if (!executedSuccessfully)
                Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
            return executedSuccessfully;
        }

        protected string BuildCharacterPrompt(IPlayableCharacter character) // TODO: custom prompt defined by player
        {
            StringBuilder sb = new StringBuilder("<");
            sb.Append($"{character.HitPoints}/{character.MaxHitPoints}Hp");
            sb.Append($" {character.MovePoints}/{character.MaxMovePoints}Mv");
            foreach (ResourceKinds resourceKinds in character.CurrentResourceKinds)
                sb.Append($" {character[resourceKinds]}/{character.MaxResource(resourceKinds)}{resourceKinds}");
            sb.Append($" {character.ExperienceToLevel}Nxt");
            if (character.Fighting != null)
                sb.Append($" {((100*character.Fighting.HitPoints)/character.Fighting.MaxHitPoints)}%");
            sb.Append(">");
            return sb.ToString();
        }
    }
}
