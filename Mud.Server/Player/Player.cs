using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.Repository;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Actor;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Player
{
    public partial class Player : ActorBase, IPlayer
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> PlayerCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => CommandHelpers.GetCommands(typeof(Player)));

        private readonly List<string> _delayedTells;
        private readonly List<CharacterData> _avatarList;

        protected readonly Dictionary<string, string> Aliases;
        protected IInputTrap<IPlayer> CurrentStateMachine;
        protected bool DeletionConfirmationNeeded;

        protected IServer Server => DependencyContainer.Instance.GetInstance<IServer>();
        protected IPlayerRepository PlayerRepository => DependencyContainer.Instance.GetInstance<IPlayerRepository>();
        protected ILoginRepository LoginRepository => DependencyContainer.Instance.GetInstance<ILoginRepository>();

        protected Player()
        {
            PlayerState = PlayerStates.Loading;

            _delayedTells = new List<string>();
            _avatarList = new List<CharacterData>();

            Aliases = new Dictionary<string, string>();
            CurrentStateMachine = null;
            DeletionConfirmationNeeded = false;
        }

        public Player(Guid id, string name)
            : this()
        {
            Id = id;
            Name = name;
        }

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => PlayerCommands.Value;

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
                    LastCommandTimestamp = DateTime.Now;
                }
                else
                {
                    LastCommand = commandLine;
                    LastCommandTimestamp = DateTime.Now;
                }

                // Extract command and parameters
                bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(Aliases, commandLine, out string command, out string rawParameters, out CommandParameter[] parameters, out bool forceOutOfGame);
                if (!extractedSuccessfully)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                    Send("Invalid command or parameters.");
                    return false;
                }

                // Execute command
                bool executedSuccessfully;
                if (forceOutOfGame || Impersonating == null)
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DisplayName, commandLine);
                    // TODO: automatically remove AFK (unless command is AFK!!) and tell if tells have been received
                    // %r%You have received tells: Type %Y%'replay'%r% to see them.%x%
                    executedSuccessfully = ExecuteCommand(command, rawParameters, parameters);
                }
                else
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Impersonating.DisplayName, commandLine);
                    executedSuccessfully = Impersonating.ExecuteCommand(command, rawParameters, parameters);
                }
                if (!executedSuccessfully)
                    Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
                return executedSuccessfully;
            }
        }

        public override bool ExecuteBeforeCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
        {
            if (methodInfo.Attribute is PlayerCommandAttribute playerCommandAttribute)
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
            if (IsAfk && methodInfo.Attribute.Name != "afk")
            {
                Send("%G%AFK%x% removed.");
                Send("%r%You have received tells: Type %Y%'replay'%r% to see them.%x%");
                IsAfk = !IsAfk;
                return true;
            }
            bool baseExecuteBeforeCommandResult = base.ExecuteBeforeCommand(methodInfo, rawParameters, parameters);
            if (baseExecuteBeforeCommandResult && methodInfo.Attribute.Name != "delete")
            {
                // once another command then 'delete' is used, reset deletion confirmation
                DeletionConfirmationNeeded = false;
            }
            return baseExecuteBeforeCommandResult;
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

        #endregion

        public event SendDataEventHandler SendData;
        public event PageDataEventHandler PageData;

        public Guid Id { get; }
        public string Name { get; }

        public string DisplayName => StringHelpers.UpperFirstLetter(Name);

        public int GlobalCooldown { get; protected set; } // delay (in Pulse) before next action

        public PlayerStates PlayerState { get; protected set; }

        public ICharacter Impersonating { get; private set; }

        public IEnumerable<CharacterData> Avatars => _avatarList;

        public IPlayer LastTeller { get; private set; }

        public IAdmin SnoopBy { get; private set; } // every messages send to 'this' will be sent to SnoopBy

        public DateTime LastCommandTimestamp { get; protected set; }
        public string LastCommand { get; protected set; }

        public virtual string Prompt => Impersonating != null 
            ? BuildCharacterPrompt(Impersonating)
            : ">";

        public bool IsAfk { get; protected set; }

        public IEnumerable<string> DelayedTells => _delayedTells; // Tell stored while AFK

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

        public void AddAvatar(CharacterData characterData)
        {
            _avatarList.Add(characterData);
        }

        public void StopImpersonating()
        {
            Impersonating?.ChangeImpersonation(null);
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

        protected string BuildCharacterPrompt(ICharacter character) // TODO: custom prompt defined by player
        {
            StringBuilder sb = new StringBuilder("<");
            sb.Append($"{character.HitPoints}/{character[SecondaryAttributeTypes.MaxHitPoints]}Hp");
            foreach (ResourceKinds resourceKinds in character.CurrentResourceKinds)
                sb.Append($" {character[resourceKinds]}/{character.GetMaxResource(resourceKinds)}{resourceKinds}");
            sb.Append($" {character.ExperienceToLevel}Nxt");
            if (character.Fighting != null)
                sb.Append($" {(int)(100d*character.Fighting.HitPoints/character.Fighting[SecondaryAttributeTypes.MaxHitPoints])}%");
            sb.Append(">");
            return sb.ToString();
        }

        protected void LoadPlayerData(PlayerData data)
        {
            Aliases.Clear();
            _avatarList.Clear();
            if (data?.Aliases != null)
            {
                foreach (KeyValuePair<string, string> alias in data.Aliases)
                    Aliases.Add(alias.Key, alias.Value);
            }

            if (data?.Characters != null)
            {
                foreach (CharacterData characterData in data.Characters)
                    _avatarList.Add(characterData);
            }
        }

        protected void FillPlayerData(PlayerData data)
        {
            data.Name = Name;
            data.Aliases = Aliases.ToDictionary(x => x.Key, x => x.Value);
            // TODO: copy from Impersonated to CharacterData
            data.Characters = _avatarList;
        }

        protected void UpdateCharacterDataFromImpersonated()
        {
            if (Impersonating == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "UpdateCharacterDataFromImpersonated while not impersonated.");
                return;
            }
            CharacterData characterData = _avatarList.FirstOrDefault(x => FindHelpers.StringEquals(x.Name, Impersonating.Name));
            if (characterData == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "UpdateCharacterDataFromImpersonated: unknown avatar {Impersonating.Name} for player {DisplayName}");
                return;
            }
            characterData.Name = Impersonating.Name;
            characterData.Sex = Impersonating.Sex;
            characterData.Class = Impersonating.Class?.Name ?? string.Empty;
            characterData.Race = Impersonating.Race?.Name ?? string.Empty;
            characterData.Level = Impersonating.Level;
            characterData.RoomId = Impersonating.Room?.Blueprint?.Id ?? 0;
            characterData.Experience = Impersonating.Experience;
            List<EquipedItemData> equipedItemDatas = new List<EquipedItemData>();
            foreach(EquipedItem equipedItem in Impersonating.Equipments.Where(x => x.Item != null))
                equipedItemDatas.Add(MapEquipedData(equipedItem));
            characterData.Equipments = equipedItemDatas;
            List<ItemData> itemDatas = new List<ItemData>();
            foreach(IItem item in Impersonating.Content)
                itemDatas.Add(MapItemData(item));
            characterData.Inventory = itemDatas;
            // TODO: aura, cooldown, quests, ...
        }

        private EquipedItemData MapEquipedData(EquipedItem equipedItem)
        {
            return new EquipedItemData
            {
                ItemId = equipedItem.Item.Blueprint.Id,
                Slot = equipedItem.Slot,
                Contains = MapContent(equipedItem.Item)
            };
        }

        private List<ItemData> MapContent(IItem item)
        {
            List<ItemData> contains = new List<ItemData>();
            if (item is IItemContainer container)
            {
                foreach (IItem subItem in container.Content)
                {
                    ItemData subItemData = MapItemData(subItem);
                    contains.Add(subItemData);
                }
            }

            return contains;
        }

        private ItemData MapItemData(IItem item)
        {
            return new ItemData
            {
                ItemId = item.Blueprint.Id,
                Contains = MapContent(item)
            };
        }

        [Command("test", Category = "!!Test!!")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating != null)
            {
                // Add quest to impersonated character is any
                QuestBlueprint questBlueprint1 = World.GetQuestBlueprint(1);
                QuestBlueprint questBlueprint2 = World.GetQuestBlueprint(2);
                ICharacter questor = World.Characters.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains("questor"));

                IQuest quest1 = new Quest.Quest(questBlueprint1, Impersonating, questor);
                Impersonating.AddQuest(quest1);
                IQuest quest2 = new Quest.Quest(questBlueprint2, Impersonating, questor);
                Impersonating.AddQuest(quest2);
            }

            return true;

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
