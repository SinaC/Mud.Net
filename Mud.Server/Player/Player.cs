using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Datas.DataContracts;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Actor;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player : ActorBase, IPlayer
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> PlayerCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => CommandHelpers.GetCommands(typeof(Player)));

        protected readonly Dictionary<string, string> Aliases;

        protected IInputTrap<IPlayer> CurrentStateMachine;

        protected Player()
        {
            Aliases = new Dictionary<string, string>();

            PlayerState = PlayerStates.Loading;
            CurrentStateMachine = null;
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
            // ! means repeat last command
            if (commandLine != null && commandLine.Length >= 1 && commandLine[0] == '!')
            {
                commandLine = LastCommand;
                LastCommandTimestamp = DateTime.Now;
            }
            else
            {
                LastCommand = commandLine;
                LastCommandTimestamp = DateTime.Now;
            }

            // If an input state machine is running, send commandLine to machine
            if (CurrentStateMachine != null && !CurrentStateMachine.IsFinalStateReached)
            {
                CurrentStateMachine.ProcessInput(this, commandLine);
                return true;
            }
            else
            {
                string command;
                string rawParameters;
                CommandParameter[] parameters;
                bool forceOutOfGame;

                // Extract command and parameters
                bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(Aliases, commandLine, out command, out rawParameters, out parameters, out forceOutOfGame);
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
        public string Name { get; protected set; }

        public string DisplayName => StringHelpers.UpperFirstLetter(Name);

        public List<ICharacter> Avatars { get; protected set; } // List of character a player can impersonate

        public int GlobalCooldown { get; protected set; } // delay (in Pulse) before next action

        public PlayerStates PlayerState { get; protected set; }

        public ICharacter Impersonating { get; private set; }

        public IPlayer LastTeller { get; private set; }

        public IAdmin SnoopBy { get; private set; } // every messages send to 'this' will be sent to SnoopBy

        public DateTime LastCommandTimestamp { get; protected set; }
        public string LastCommand { get; protected set; }

        public virtual string Prompt => Impersonating != null 
            ? BuildCharacterPrompt(Impersonating)
            : ">";

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
            Name = name;
            Aliases.Clear();

            PlayerData data = Repository.PlayerManager.Load(name);
            if (data?.Aliases != null)
            {
                foreach (CoupledData<string, string> alias in data.Aliases)
                    Aliases.Add(alias.Key, alias.Data);
            }

            // TODO: impersonate list

            PlayerState = PlayerStates.Playing;
            return true;
        }

        public virtual bool Save()
        {
            PlayerData data = new PlayerData
            {
                Name = Name,
                Aliases = Aliases.Select(x => new CoupledData<string, string> { Key = x.Key, Data = x.Value }).ToList(),
                Characters = new List<CharacterData>
                {
                    new CharacterData
                    {
                        Name = "sinac",
                        RoomId = 3001,
                        Level = 20,
                        Sex = Sex.Male,
                        Class = "priest",
                        Race = "elf",
                        // TODO: impersonate list
                        PrimaryAttributes = new Dictionary<PrimaryAttributeTypes, int>
                        {
                            {PrimaryAttributeTypes.Strength, 50},
                            {PrimaryAttributeTypes.Intellect, 60},
                            {PrimaryAttributeTypes.Spirit, 70},
                            {PrimaryAttributeTypes.Agility, 80},
                            {PrimaryAttributeTypes.Stamina, 90},
                        }.Select(x => new CoupledData<PrimaryAttributeTypes,int> { Key = x.Key, Data = x.Value}).ToList(),
                    }
                }
            };

            Repository.PlayerManager.Save(data);

            return true;
        }

        public void SetLastTeller(IPlayer teller)
        {
            LastTeller = teller;
        }

        public void SetSnoopBy(IAdmin snooper)
        {
            SnoopBy = snooper;
        }

        public void StopImpersonating()
        {
            Impersonating?.ChangeImpersonation(null);
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

        #endregion

        protected string BuildCharacterPrompt(ICharacter character)
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

        [Command("test", Category = "!!Test!!")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            //Send("Player: DoTest" + Environment.NewLine);
            //StringBuilder lorem = new StringBuilder("1/Lorem ipsum dolor sit amet, " + Environment.NewLine +
            //                                        "2/consectetur adipiscing elit, " + Environment.NewLine +
            //                                        "3/sed do eiusmod tempor incididunt " + Environment.NewLine +
            //                                        "4/ut labore et dolore magna aliqua. " + Environment.NewLine +
            //                                        "5/Ut enim ad minim veniam, " + Environment.NewLine +
            //                                        "6/quis nostrud exercitation ullamco " + Environment.NewLine +
            //                                        "7/laboris nisi ut aliquip ex " + Environment.NewLine +
            //                                        "8/ea commodo consequat. " + Environment.NewLine +
            //                                        "9/Duis aute irure dolor in " + Environment.NewLine +
            //                                        "10/reprehenderit in voluptate velit " + Environment.NewLine +
            //                                        "11/esse cillum dolore eu fugiat " + Environment.NewLine +
            //                                        "12/nulla pariatur. " + Environment.NewLine +
            //                                        "13/Excepteur sint occaecat " + Environment.NewLine +
            //                                        "14/cupidatat non proident, " + Environment.NewLine +
            //                                        "15/sunt in culpa qui officia deserunt " + Environment.NewLine //+
            //                                        //"16/mollit anim id est laborum." + Environment.NewLine
            //                                        );
            //Page(lorem);
            string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            StringBuilder sb = new StringBuilder();
            foreach (string word in lorem.Split(' ', ',', ';', '.'))
                sb.AppendLine(word);
            Page(sb);
            return true;
        }
    }
}
