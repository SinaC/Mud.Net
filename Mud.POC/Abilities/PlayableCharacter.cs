using System;
using System.Collections.Generic;
using System.Linq;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.POC.Affects;
using Mud.Server.Random;
using Mud.Server.Interfaces.GameAction;
using Mud.Common;
using Mud.Server.GameAction;

namespace Mud.POC.Abilities
{
    public partial class PlayableCharacter : IPlayableCharacter
    {
        private static readonly Lazy<IReadOnlyTrie<IGameActionInfo>> PlayableCharacterCommands = new Lazy<IReadOnlyTrie<IGameActionInfo>>(() => null);

        private IRandomManager RandomManager { get; }
        private IAbilityManager AbilityManager { get; }
        private IAttributeTableManager AttributeTableManager { get; }
        private IGameActionManager GameActionManager { get; }

        private List<KnownAbility> _knownAbilities;

        public PlayableCharacter(IRandomManager randomManager, IAbilityManager abilityManager, IAttributeTableManager attributeTableManager, IGameActionManager gameActionManager)
        {
            RandomManager = randomManager;
            AbilityManager = abilityManager;
            AttributeTableManager = attributeTableManager;
            GameActionManager = gameActionManager;
            Experience = 1000;
            HitPoints = 1000;
            Level = 1;
            Position = Positions.Standing;
            _knownAbilities = new List<KnownAbility>();
        }

        public PlayableCharacter(IRandomManager randomManager, IAbilityManager abilityManager, IAttributeTableManager attributeTableManager, IGameActionManager gameActionManager, IEnumerable<KnownAbility> knownAbilities, long experience, int hp, int level, Positions position)
            :this(randomManager, abilityManager, attributeTableManager, gameActionManager)
        {
            _knownAbilities = knownAbilities.ToList();
            Experience = experience;
            HitPoints = hp;
            Level = level;
            Position = position;
        }

        public IReadOnlyTrie<IGameActionInfo> Commands => PlayableCharacterCommands.Value;

        public string Name { get; }
        public string DebugName { get; }
        public IEnumerable<string> Keywords { get; }
        public int Level { get; }
        public int HitPoints { get; }
        public Positions Position { get; }
        public IClass Class { get; }
        public IRace Race { get; }
        public IRoom Room { get; }
        public ICharacter Fighting { get; }
        public IEnumerable<IItem> Inventory { get; }
        public IEnumerable<IItem> Equipments { get; }
        public IEnumerable<KnownAbility> KnownAbilities { get; }

        public int LearnedAbility(string name)
        {
            return _knownAbilities.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Ability.Name, name))?.Learned ?? 0;
        }

        public int LearnedAbility(IAbility ability)
        {
            return _knownAbilities.FirstOrDefault(x => x.Ability == ability)?.Learned ?? 0;
        }

        public int this[ResourceKinds resource] => throw new NotImplementedException();

        public IEnumerable<ResourceKinds> CurrentResourceKinds { get; }

        public CharacterFlags BaseCharacterFlags { get; }
        public CharacterFlags CurrentCharacterFlags { get; }

        public int BaseAttributes(CharacterAttributes attribute)
        {
            throw new NotImplementedException();
        }

        public int CurrentAttributes(CharacterAttributes attribute)
        {
            throw new NotImplementedException();
        }

        public int GetMaxResource(ResourceKinds resource)
        {
            throw new NotImplementedException();
        }

        public void UpdateResource(ResourceKinds resource, int amount)
        {
            throw new NotImplementedException();
        }

        public IAura GetAura(int abilityId)
        {
            throw new NotImplementedException();
        }

        public IAura GetAura(string abilityName)
        {
            throw new NotImplementedException();
        }

        public IAura GetAura(IAbility ability)
        {
            throw new NotImplementedException();
        }

        public bool MultiHit(ICharacter enemy)
        {
            throw new NotImplementedException();
        }

        public bool WeaponDamage(ICharacter source, IItemWeapon weapon, int damage, SchoolTypes damageType, bool visible)
        {
            throw new NotImplementedException();
        }

        public bool AbilityDamage(IEntity source, IAbility ability, int damage, SchoolTypes damageType, bool visible)
        {
            throw new NotImplementedException();
        }


        public void Send(string msg, params object[] args)
        {
            Log.Default.WriteLine(LogLevels.Debug, msg, args);
        }

        public void Act(ActOptions option, string format, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public void ActToNotVictim(ICharacter victim, string format, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        //
        public long Experience { get; private set; }

        public bool CheckAbilityImprove(IAbility ability, bool abilityUsedSuccessfully, int multiplier)
        {
            KnownAbility knownAbility = KnownAbilities.FirstOrDefault(x => x.Ability == ability);
            return CheckAbilityImprove(knownAbility, abilityUsedSuccessfully, multiplier);
        }

        public bool CheckAbilityImprove(KnownAbility knownAbility, bool abilityUsedSuccessfully, int multiplier)
        {
            // Know ability ?
            if (knownAbility == null
                || knownAbility.Ability == null
                || knownAbility.Learned == 0
                || knownAbility.Learned == 100)
                return false; // ability not known
            // check to see if the character has a chance to learn
            if (multiplier <= 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "PlayableCharacter.CheckAbilityImprove: multiplier had invalid value {0}", multiplier);
                multiplier = 1;
            }
            int difficultyMultiplier = knownAbility.Rating;
            if (difficultyMultiplier <= 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "PlayableCharacter.CheckAbilityImprove: difficulty multiplier had invalid value {0} for KnownAbility {1} Player {2}", multiplier, knownAbility.Ability, DebugName);
                difficultyMultiplier = 1;
            }
            int chance = 10 * AttributeTableManager.GetLearnPercentage(this) / (multiplier * difficultyMultiplier * 4) + Level;
            if (RandomManager.Range(1, 1000) > chance)
                return false;
            // now that the character has a CHANCE to learn, see if they really have
            if (abilityUsedSuccessfully)
            {
                chance = (100 - knownAbility.Learned).Range(5, 95);
                if (RandomManager.Chance(chance))
                {
                    Send("You have become better at {0}!", knownAbility.Ability.Name);
                    knownAbility.Learned++;
                    GainExperience(2*difficultyMultiplier);
                    return true;
                }
            }
            else
            {
                chance = (knownAbility.Learned/2).Range(5, 30);
                if (RandomManager.Chance(chance))
                {
                    Send("You learn from your mistakes, and your {0} skill improves.!", knownAbility.Ability.Name);
                    int learned = RandomManager.Range(1, 3);
                    knownAbility.Learned = Math.Min(knownAbility.Learned + learned, 100);
                    GainExperience(2 * difficultyMultiplier);
                    return true;
                }
            }

            return false;
        }

        public void GainExperience(long experience)
        {
            Experience += experience;
        }


        //
        public bool ExecuteCommand(string command, string rawParameters, ICommandParameter[] parameters)
        {
            // Search for command and invoke it
            if (Commands != null)
            {
                command = command.ToLowerInvariant(); // lower command
                List<TrieEntry<IGameActionInfo>> methodInfos = Commands.GetByPrefix(command).ToList();
                TrieEntry<IGameActionInfo> entry = methodInfos.OrderBy(x => x.Value.Priority).FirstOrDefault(); // use priority to choose between conflicting commands
                if (entry.Value?.NoShortcut == true && command != entry.Key) // if command doesn't accept shortcut, inform player
                {
                    Send("If you want to {0}, spell it out.", entry.Key.ToUpper());
                    return true;
                }
                if (entry.Value is IGameActionInfo gai && gai.CommandExecutionType != null)
                {
                    // TODO: awful casting
                    string executionResults = GameActionManager.Execute(gai, (Mud.Server.Interfaces.Actor.IActor)null, command, rawParameters, parameters);
                    if (executionResults != null)
                    {
                        Send(executionResults);
                        return false;
                    }
                    return true;
                }
                else
                {
                    Log.Default.WriteLine(LogLevels.Warning, $"Command {command} not found");
                    Send("Command not found.");
                    return false;
                }
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, $"No command found for {GetType().FullName}");
                Send("Command not found.");
                return false;
            }
        }

        private CommandExecutionResults ConvertToCommandExecutionResults(string command, object rawResult)
        {
            if (rawResult == null)
                return CommandExecutionResults.Ok;
            if (rawResult is bool boolResult)
                return boolResult
                    ? CommandExecutionResults.Ok
                    : CommandExecutionResults.Error;
            if (rawResult is CommandExecutionResults commandExecutionResult)
                return commandExecutionResult;
            Log.Default.WriteLine(LogLevels.Error, "Command {0} return type {1} is not convertible to CommandExecutionResults", command, rawResult.GetType().Name);
            return CommandExecutionResults.Ok;
        }
    }
}
