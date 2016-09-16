using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Datas.DataContracts;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Constants;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin : Player.Player, IAdmin
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> AdminCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => CommandHelpers.GetCommands(typeof(Admin)));

        public Admin(Guid id, string name) 
            : base(id, name)
        {
        }

        #region IAdmin

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => AdminCommands.Value;

        public override bool ProcessCommand(string commandLine) // TODO: refactoring needed: almost same code in Player
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
                    Send("Invalid command or parameters");
                    return false;
                }

                // Execute command
                bool executedSuccessfully;
                if (forceOutOfGame || (Impersonating == null && Incarnating == null)) // neither incarnating nor impersonating
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DisplayName, commandLine);
                    executedSuccessfully = ExecuteCommand(command, rawParameters, parameters);
                }
                else if (Incarnating != null) // incarnating
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Incarnating.DebugName, commandLine);
                    executedSuccessfully = Incarnating.ExecuteCommand(command, rawParameters, parameters);
                }
                else if (Impersonating != null) // impersonating
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Impersonating.DebugName, commandLine);
                    executedSuccessfully = Impersonating.ExecuteCommand(command, rawParameters, parameters);
                }
                else
                {
                    Log.Default.WriteLine(LogLevels.Error, "[{0}] is neither out of game, nor impersonating, nor incarnating");
                    executedSuccessfully = false;
                }
                if (!executedSuccessfully)
                    Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
                return executedSuccessfully;
            }
        }

        #endregion

        public override string Prompt => Incarnating != null 
            ? BuildIncarnatePrompt()
            : base.Prompt;

        public override bool Load(string name)
        {
            Name = name;
            Aliases.Clear();

            //// TODO: load player file
            //// TODO: load impersonation list
            //// TODO: load aliases

            //// Aliases
            //Aliases.Add("i1", "/impersonate mob1");
            //Aliases.Add("i4", "/impersonate mob4");
            //Aliases.Add("t1", "/force mob2 test 3 mob1");
            //Aliases.Add("t2", "/force mob4 test 4 mob1");
            //Aliases.Add("sh", "test 'power word: shield'");
            //Aliases.Add("fo", "/force mob2 follow mob1");

            //Aliases.Add("1", "/force hassan follow mob2");
            //Aliases.Add("2", "follow hassan");
            //Aliases.Add("3", "/force hassan group mob1");
            //Aliases.Add("4", "/force mob2 group hassan");

            //Save(); // Test purpose

            ////
            //PlayerState = PlayerStates.Playing;
            //return true;


            AdminData data = Repository.AdminManager.Load(name);
            if (data?.Aliases != null)
            {
                foreach (CoupledData<string, string> alias in data.Aliases)
                    Aliases.Add(alias.Key, alias.Data);
            }

            // TODO: impersonate list

            PlayerState = PlayerStates.Playing;
            return true;
        }

        public override bool Save()
        {
            AdminData data = new AdminData
            {
                Name = Name,
                Aliases = Aliases.Select(x => new CoupledData<string,string> { Key = x.Key, Data = x.Value}).ToList(),
                Characters = new List<CharacterData>
                {
                    new CharacterData
                    {
                        Name = "sinac",
                        RoomId = 3000,
                        Level = 30,
                        Sex = Sex.Male,
                        Class = "druid",
                        Race = "troll",
                        // TODO: impersonate list
                        PrimaryAttributes = new Dictionary<PrimaryAttributeTypes, int>
                        {
                            {PrimaryAttributeTypes.Strength, 100},
                            {PrimaryAttributeTypes.Intellect, 110},
                            {PrimaryAttributeTypes.Spirit, 120},
                            {PrimaryAttributeTypes.Agility, 130},
                            {PrimaryAttributeTypes.Stamina, 140},
                        }.Select(x => new CoupledData<PrimaryAttributeTypes,int> { Key = x.Key, Data = x.Value}).ToList(),
                    }
                }
            };
            Repository.AdminManager.Save(data);

            return true;
        }

        public override void OnDisconnected()
        {
            base.OnDisconnected();

            // Stop incarnation if any
            if (Incarnating != null)
            {
                StopIncarnating();
            }
        }

        #endregion

        public IEntity Incarnating { get; private set; }

        public void StopIncarnating()
        {
            Incarnating?.ChangeIncarnation(null);
            Incarnating = null;
        }

        #endregion

        private string BuildIncarnatePrompt()
        {
            ICharacter character = Incarnating as ICharacter;
            if (character != null)
                return BuildCharacterPrompt(character);
            return $"{Incarnating.DisplayName}>";
        }
    }
}
