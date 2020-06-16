using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.Repository;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Actor;
using Mud.Server.Input;
using Mud.Server.Common;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Admin;
using Mud.Common;

namespace Mud.Server.Player
{
    public partial class Player : ActorBase, IPlayer
    {
        private static readonly Lazy<IReadOnlyTrie<CommandExecutionInfo>> PlayerCommands = new Lazy<IReadOnlyTrie<CommandExecutionInfo>>(GetCommands<Player>);

        private readonly List<string> _delayedTells;
        private readonly List<PlayableCharacterData> _avatarList;
        private readonly Dictionary<string, string> _aliases;

        protected IInputTrap<IPlayer> CurrentStateMachine;
        protected bool DeletionConfirmationNeeded;

        protected IPlayerManager PlayerManager => DependencyContainer.Current.GetInstance<IPlayerManager>();
        protected IServerPlayerCommand ServerPlayerCommand => DependencyContainer.Current.GetInstance<IServerPlayerCommand>();
        protected IPlayerRepository PlayerRepository => DependencyContainer.Current.GetInstance<IPlayerRepository>();
        protected ILoginRepository LoginRepository => DependencyContainer.Current.GetInstance<ILoginRepository>();
        protected ITimeManager TimeHandler => DependencyContainer.Current.GetInstance<ITimeManager>();

        protected Player()
        {
            PlayerState = PlayerStates.Loading;

            _delayedTells = new List<string>();
            _avatarList = new List<PlayableCharacterData>();
            _aliases = new Dictionary<string, string>();

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

        // Used for promotion
        public Player(Guid id, string name, IReadOnlyDictionary<string, string> aliases, IEnumerable<PlayableCharacterData> avatarList) : this(id, name)
        {
            _aliases = aliases?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();
            _avatarList = avatarList?.ToList() ?? new List<PlayableCharacterData>();
        }

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<CommandExecutionInfo> Commands => PlayerCommands.Value;

        public override bool ProcessCommand(string commandLine)
        {
            // If an input state machine is running, send commandLine to machine
            if (CurrentStateMachine != null && !CurrentStateMachine.IsFinalStateReached)
            {
                CurrentStateMachine.ProcessInput(this, commandLine);
                return true;
            }
            else
            {
                CurrentStateMachine = null; // reset current state machine if not currently running one
                // ! means repeat last command (only when last command was not delete)
                if (commandLine != null && commandLine.Length >= 1 && commandLine[0] == '!')
                {
                    if (LastCommand?.ToLowerInvariant() == "delete")
                    {
                        Send("Cannot use '!' to repeat 'delete' command");
                        DeletionConfirmationNeeded = false; // reset delete confirmation
                        return false;
                    }
                    commandLine = LastCommand;
                    LastCommandTimestamp = TimeHandler.CurrentTime;
                }
                else
                {
                    LastCommand = commandLine;
                    LastCommandTimestamp = TimeHandler.CurrentTime;
                }

                // Extract command and parameters
                bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(
                    isForceOutOfGame => isForceOutOfGame || Impersonating == null 
                        ? Aliases
                        : Impersonating?.Aliases,
                    commandLine,
                    out string command, out string rawParameters, out CommandParameter[] parameters, out bool forceOutOfGame);
                if (!extractedSuccessfully)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                    Send("Invalid command or parameters.");
                    return false;
                }

                // Execute command
                return InnerExecuteCommand(commandLine, command, rawParameters, parameters, forceOutOfGame);
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

        public PlayerStates PlayerState { get; protected set; }

        public IPlayableCharacter Impersonating { get; private set; }

        public IEnumerable<PlayableCharacterData> Avatars => _avatarList;

        public IReadOnlyDictionary<string, string> Aliases => _aliases;

        public IPlayer LastTeller { get; private set; }

        public IAdmin SnoopBy { get; private set; } // every messages send to 'this' will be sent to SnoopBy

        public DateTime LastCommandTimestamp { get; protected set; }
        public string LastCommand { get; protected set; }

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

        public void ResetDeletionConfirmation()
        {
            DeletionConfirmationNeeded = false;
        }

        public void DecreaseGlobalCooldown() // decrease one by one
        {
            GlobalCooldown = Math.Max(GlobalCooldown - 1, 0);
        }

        public void SetGlobalCooldown(int pulseCount) // set global cooldown delay (in pulse)
        {
            GlobalCooldown = pulseCount;
        }

        public virtual bool Load(string name)
        {
            PlayerData data = PlayerRepository.Load(name);
            // Load player data
            LoadPlayerData(data);
            //
            PlayerState = PlayerStates.Playing;
            return true;
        }

        public virtual bool Save()
        {
            if (Impersonating != null)
                UpdateCharacterDataFromImpersonated();
            //
            PlayerData data = new PlayerData();
            // Fill player data
            FillPlayerData(data);
            //
            PlayerRepository.Save(data);
            //
            Log.Default.WriteLine(LogLevels.Info, $"Player {DisplayName} saved");
            return true;
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

        public void StopImpersonating()
        {
            Impersonating?.StopImpersonation();
            World.RemoveCharacter(Impersonating); // extract avatar  TODO: linkdead instead of RemoveCharacter ?
            Impersonating = null;
            PlayerState = PlayerStates.Playing;
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

        public virtual StringBuilder PerformSanityCheck()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("--Player--");
            sb.AppendLine($"Id: {Id}");
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"DisplayName: {DisplayName}");
            sb.AppendLine($"LastCommand: {LastCommand} Timestamp: {LastCommandTimestamp}");
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

        #region ActorBase

        protected override bool ExecuteBeforeCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
        {
            if (methodInfo.CommandAttribute is PlayerCommandAttribute playerCommandAttribute)
            {
                if (playerCommandAttribute.MustBeImpersonated && Impersonating == null)
                {
                    Send($"You must be impersonated to use '{playerCommandAttribute.Name}'.");
                    return false;
                }

                if (playerCommandAttribute.CannotBeImpersonated && Impersonating != null)
                {
                    Send($"You cannot be impersonated to use '{playerCommandAttribute.Name}'.");
                    return false;
                }
            }
            if (IsAfk && methodInfo.CommandAttribute.Name != "afk")
            {
                Send("%G%AFK%x% removed.");
                Send("%r%You have received tells: Type %Y%'replay'%r% to see them.%x%");
                IsAfk = !IsAfk;
                return true;
            }
            bool baseExecuteBeforeCommandResult = base.ExecuteBeforeCommand(methodInfo, rawParameters, parameters);
            if (baseExecuteBeforeCommandResult && methodInfo.CommandAttribute.Name != "delete")
            {
                // once another command then 'delete' is used, reset deletion confirmation
                DeletionConfirmationNeeded = false;
            }
            return baseExecuteBeforeCommandResult;
        }

        #endregion

        protected virtual bool InnerExecuteCommand(string commandLine, string command, string rawParameters, CommandParameter[] parameters, bool forceOutOfGame)
        {
            // Execute command
            bool executedSuccessfully;
            if (forceOutOfGame || Impersonating == null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DisplayName, commandLine);
                executedSuccessfully = ExecuteCommand(command, rawParameters, parameters);
            }
            else if (Impersonating != null) // impersonating
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Impersonating.DebugName, commandLine);
                executedSuccessfully = Impersonating.ExecuteCommand(command, rawParameters, parameters);
            }
            else
            {
                Wiznet.Wiznet($"[{DisplayName}] is neither out of game nor impersonating", WiznetFlags.Bugs, AdminLevels.Implementor);
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

        protected void LoadPlayerData(PlayerData data)
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

        protected void FillPlayerData(PlayerData data)
        {
            data.Name = Name;
            data.PagingLineCount = PagingLineCount;
            data.Aliases = Aliases.ToDictionary(x => x.Key, x => x.Value);
            // TODO: copy from Impersonated to PlayableCharacterData
            data.Characters = _avatarList.ToArray();
        }

        protected void UpdateCharacterDataFromImpersonated()
        {
            if (Impersonating == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "UpdateCharacterDataFromImpersonated while not impersonated.");
                return;
            }
            int index = _avatarList.FindIndex(x => StringCompareHelpers.StringEquals(x.Name, Impersonating.Name));
            if (index < 0)
            {
                Wiznet.Wiznet($"UpdateCharacterDataFromImpersonated: unknown avatar {Impersonating.DebugName} for player {DisplayName}", WiznetFlags.Bugs, AdminLevels.Implementor);
                return;
            }

            PlayableCharacterData updatedCharacterData = Impersonating.MapPlayableCharacterData();
            _avatarList[index] = updatedCharacterData; // replace with new character data
        }

        [Command("test", "!!Test!!")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            TableGenerator<Tuple<string,string,int>> generator = new TableGenerator<Tuple<string, string, int>>();
            generator.AddColumn("Header1", 10, tuple => tuple.Item1);
            generator.AddColumn("Header2", 15, tuple => tuple.Item2);
            generator.AddColumn("Header3", 8, tuple => tuple.Item3.ToString());
            StringBuilder sb = generator.Generate("Test column duplicate", 3, Enumerable.Range(0, 50).Select(x => new Tuple<string, string, int>("Value1_" + x.ToString(), "Value2_" + (50 - x).ToString(), x)));
            Send(sb);

            return true;

            //if (Impersonating != null)
            //{
            //    // Add quest to impersonated character is any
            //    QuestBlueprint questBlueprint1 = World.GetQuestBlueprint(1);
            //    QuestBlueprint questBlueprint2 = World.GetQuestBlueprint(2);
            //    INonPlayableCharacter questor = World.NonPlayableCharacters.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains("questor"));

            //    IQuest quest1 = new Quest.Quest(questBlueprint1, Impersonating, questor);
            //    Impersonating.AddQuest(quest1);
            //    IQuest quest2 = new Quest.Quest(questBlueprint2, Impersonating, questor);
            //    Impersonating.AddQuest(quest2);
            //}

            //return true;

            //IQuest quest1 = new Quest.Quest(questBlueprint1, mob1, mob2);
            //mob1.AddQuest(quest1);
            //IQuest quest2 = new Quest.Quest(questBlueprint2, mob1, mob2);
            //mob1.AddQuest(quest2);

            ////Send("Player: DoTest" + Environment.NewLine);
            ////StringBuilder lorem = new StringBuilder("1/Lorem ipsum dolor sit amet, " + Environment.NewLine +
            ////                                        "2/consectetur adipiscing elit, " + Environment.NewLine +
            ////                                        "3/sed do eiusmod tempor incididunt " + Environment.NewLine +
            ////                                        "4/ut labore et dolore magna aliqua. " + Environment.NewLine +
            ////                                        "5/Ut enim ad minim veniam, " + Environment.NewLine +
            ////                                        "6/quis nostrud exercitation ullamco " + Environment.NewLine +
            ////                                        "7/laboris nisi ut aliquip ex " + Environment.NewLine +
            ////                                        "8/ea commodo consequat. " + Environment.NewLine +
            ////                                        "9/Duis aute irure dolor in " + Environment.NewLine +
            ////                                        "10/reprehenderit in voluptate velit " + Environment.NewLine +
            ////                                        "11/esse cillum dolore eu fugiat " + Environment.NewLine +
            ////                                        "12/nulla pariatur. " + Environment.NewLine +
            ////                                        "13/Excepteur sint occaecat " + Environment.NewLine +
            ////                                        "14/cupidatat non proident, " + Environment.NewLine +
            ////                                        "15/sunt in culpa qui officia deserunt " + Environment.NewLine //+
            ////                                        //"16/mollit anim id est laborum." + Environment.NewLine
            ////                                        );
            ////Page(lorem);
            //string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            //StringBuilder sb = new StringBuilder();
            //foreach (string word in lorem.Split(' ', ',', ';', '.'))
            //    sb.AppendLine(word);
            //Page(sb);
            //return true;
        }
    }
}
