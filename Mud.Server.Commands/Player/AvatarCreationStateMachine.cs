using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Common;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.TableGenerator;
using System.Text;
using System.Text.RegularExpressions;

namespace Mud.Server.Commands.Player;

public enum AvatarCreationStates
{
    NameChoice, // -> NameConfirmation | NameChoice | Quit
    NameConfirmation, // -> NameChoice | SexChoice | Quit
    SexChoice, // -> SexChoice | RaceChoice | Quit
    RaceChoice, // -> RaceChoice | ClassChoice | Quit
    ClassChoice, // -> ClassChoice | CustomizationChoice | Quit
    CustomizationChoice, // -> WeaponChoice | Customize | Quit
    Customize, // -> Customize | WeaponChoice | Quit
    WeaponChoice, // -> CreationComplete | ImmediateImpersonate | Quit
    ImmediateImpersonate, // -> CreationComplete
    CreationComplete,
    Quit
}

//list,learned,premise,add,drop,info,help, and done

internal class AvatarCreationStateMachine : InputTrapBase<IPlayer, AvatarCreationStates>
{
    private string? _name;
    private Sex? _sex;
    private IPlayableRace? _race;
    private IClass? _class;
    private int _creationPoints;
    private List<IAbilityUsage> _learnedAbilities = [];
    private List<IAbilityGroupUsage> _learnedAbilityGroups = [];
    private List<IAbilityUsage> _chosenAbilities = [];
    private List<IAbilityGroupUsage> _chosenAbilityGroups = [];

    private ILogger Logger { get; }
    private IServerPlayerCommand ServerPlayerCommand { get; }
    private IRaceManager RaceManager { get; }
    private IClassManager ClassManager { get; }
    private IUniquenessManager UniquenessManager { get; }
    private ITimeManager TimeManager { get; }
    private IRoomManager RoomManager { get; }
    private IFlagFactory<IShieldFlags, IShieldFlagValues> ShieldFlagFactory { get; }
    private IGameActionManager GameActionManager { get; }
    private ICommandParser CommandParser { get; }
    private IAbilityGroupManager AbilityGroupManager { get; }

    public override bool IsFinalStateReached => State == AvatarCreationStates.CreationComplete || State == AvatarCreationStates.Quit;

    public AvatarCreationStateMachine(ILogger logger, IServerPlayerCommand serverPlayerCommand, IRaceManager raceManager, IClassManager classManager, IUniquenessManager uniquenessManager, ITimeManager timeManager, IRoomManager roomManager, IFlagFactory<IShieldFlags, IShieldFlagValues> shieldFlagFactory, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityGroupManager abilityGroupManager)
    {
        Logger = logger;
        ServerPlayerCommand = serverPlayerCommand;
        RaceManager = raceManager;
        ClassManager = classManager;
        UniquenessManager = uniquenessManager;
        TimeManager = timeManager;
        RoomManager = roomManager;
        ShieldFlagFactory = shieldFlagFactory;
        GameActionManager = gameActionManager;
        CommandParser = commandParser;
        AbilityGroupManager = abilityGroupManager;

        KeepInputAsIs = false;
        StateMachine = new Dictionary<AvatarCreationStates, Func<IPlayer, string, AvatarCreationStates>>
        {
            {AvatarCreationStates.NameChoice, ProcessNameChoice},
            {AvatarCreationStates.NameConfirmation, ProcessNameConfirmation},
            {AvatarCreationStates.SexChoice, ProcessSexChoice},
            {AvatarCreationStates.RaceChoice, ProcessRaceChoice},
            {AvatarCreationStates.ClassChoice, ProcessClassChoice},
            {AvatarCreationStates.CustomizationChoice, ProcessCustomizationChoice},
            {AvatarCreationStates.Customize, ProcessCustomize},
            {AvatarCreationStates.WeaponChoice, ProcessWeaponChoice},
            {AvatarCreationStates.ImmediateImpersonate, ProcessImmediateImpersonate},
            {AvatarCreationStates.CreationComplete, ProcessCreationComplete},
            {AvatarCreationStates.Quit, ProcessQuit}
        };
        State = AvatarCreationStates.NameChoice;
    }

    private AvatarCreationStates ProcessNameChoice(IPlayer player, string input)
    {
        player.ChangePlayerState(PlayerStates.CreatingAvatar);
        if (!string.IsNullOrWhiteSpace(input))
        {
            if (input == "quit")
            {
                player.Send("Creation cancelled.");
                return AvatarCreationStates.Quit;
            }
            if (input.Contains(' ') || !Regex.IsMatch(input, @"^[a-z]+$"))
            {
                player.Send("Invalid characters detected.");
                player.Send("Please enter a VALID name (type quit to stop creation):");
                return AvatarCreationStates.NameChoice;
            }
            if (player.Avatars.Any(x => StringCompareHelpers.StringEquals(x.Name, input)))
            {
                player.Send("You already have an avatar with that name.");
                player.Send("Please enter a name (type quit to stop creation):");
                return AvatarCreationStates.NameChoice;
            }
            // Can create an avatar with login name
            if (input != player.Name && !UniquenessManager.IsAvatarNameAvailable(input))
            {
                player.Send("This avatar name is not available.");
                player.Send("Please enter a name (type quit to stop creation):");
                return AvatarCreationStates.NameChoice;
            }
            _name = input;
            player.Send("Are you sure '{0}' is the name of your avatar? (y/n/quit)", _name.UpperFirstLetter());
            return AvatarCreationStates.NameConfirmation;
        }
        player.Send("Please enter a name (type quit to stop creation):");
        return AvatarCreationStates.NameChoice;
    }

    private AvatarCreationStates ProcessNameConfirmation(IPlayer player, string input)
    {
        if (input == "y" || input == "yes")
        {
            player.Send("Nice! Please choose a sex (type quit to stop creation).");
            DisplaySexList(player);
            return AvatarCreationStates.SexChoice;
        }
        else if (input == "quit")
        {
            player.Send("Creation cancelled.");
            return AvatarCreationStates.Quit;
        }
        player.Send("Ok, what name would you give to your avatar (type quit to stop creation)?");
        return AvatarCreationStates.NameChoice;
    }

    private AvatarCreationStates ProcessSexChoice(IPlayer player, string input)
    {
        if (input == "quit")
        {
            player.Send("Creation cancelled.");
            return AvatarCreationStates.Quit;
        }
        var found = EnumHelpers.TryFindByPrefix(input, out Sex sex);
        if (found)
        {
            _sex = sex;
            player.Send("Great! Please choose a race (type quit to stop creation).");
            DisplayRaceList(player, false);
            return AvatarCreationStates.RaceChoice;
        }
        DisplaySexList(player);
        return AvatarCreationStates.SexChoice;
    }

    private AvatarCreationStates ProcessRaceChoice(IPlayer player, string input)
    {
        if (input == "quit")
        {
            player.Send("Creation cancelled.");
            return AvatarCreationStates.Quit;
        }
        var races = RaceManager.PlayableRaces.Where(x => StringCompareHelpers.StringStartsWith(x.Name, input)).ToList();
        if (races.Count == 1)
        {
            _race = races[0];
            _creationPoints = _race.CreationPointsStartValue;
            player.Send("Good choice! Now, please choose a class (type quit to stop creation).");
            DisplayClassList(player, false);
            return AvatarCreationStates.ClassChoice;
        }
        DisplayRaceList(player);
        return AvatarCreationStates.RaceChoice;
    }

    private AvatarCreationStates ProcessClassChoice(IPlayer player, string input)
    {
        if (input == "quit")
        {
            player.Send("Creation cancelled.");
            return AvatarCreationStates.Quit;
        }
        var classes = ClassManager.Classes.Where(x => StringCompareHelpers.StringStartsWith(x.Name, input)).ToList();
        if (classes.Count == 1)
        {
            _class = classes[0];
            // add basic ability groups
            foreach (var basicAbilityGroup in _class.BasicAbilityGroups)
            {
                _learnedAbilityGroups.Add(basicAbilityGroup);
            }
            // customization ?
            player.Send("Do you wish to customize this character?");
            player.Send("Customization takes time, but allows a wider range of skills and abilities.");
            player.Send("Customize (Y/N)?");
            return AvatarCreationStates.CustomizationChoice;

        }
        DisplayClassList(player);
        return AvatarCreationStates.ClassChoice;
    }

    private AvatarCreationStates ProcessCustomizationChoice(IPlayer player, string input)
    {
        if (input == "quit")
        {
            player.Send("Creation cancelled.");
            return AvatarCreationStates.Quit;
        }

        if (input == "y" || input == "yes")
        {
            DisplayCustomizeList(player);
            // display choices
            player.Send("Choice (add,drop,list,learned,premise,help,info,done)");
            return AvatarCreationStates.Customize;
        }
        else if (input == "n" || input == "no")
        {
            // add default ability groups
            foreach (var defaultAbilityGroup in _class!.DefaultAbilityGroups)
            {
                _learnedAbilityGroups.Add(defaultAbilityGroup);
            }
            // add abilities found in groups
            foreach (var learnedAbilityGroup in _learnedAbilityGroups)
            {
                AddLearnedAbilitiesFromLearnedAbilityGroup(learnedAbilityGroup.AbilityGroupDefinition);
            }

            player.Send("Please pick a weapon from the following choices:");
            DisplayWeaponList(player, false);
            return AvatarCreationStates.WeaponChoice;
        }
        player.Send("Please answer (Y/N)?");
        return AvatarCreationStates.CustomizationChoice;
    }

    private AvatarCreationStates ProcessCustomize(IPlayer player, string input)
    {
        if (input == "quit")
        {
            player.Send("Creation cancelled.");
            return AvatarCreationStates.Quit;
        }

        // available choices: add/drop/list/learned/premise/help/info/done
        if (input == "done")
        {
            // add chosen abilities to learned abilities
            foreach (var chosenAbility in _chosenAbilities)
            {
                if (!_learnedAbilities.Contains(chosenAbility))
                    _learnedAbilities.Add(chosenAbility);
            }
            // add chosen groups to learned groups
            foreach (var chosenAbilityGroup in _chosenAbilityGroups)
            {
                if (!_chosenAbilityGroups.Contains(chosenAbilityGroup))
                    _learnedAbilityGroups.Add(chosenAbilityGroup);
            }
            // add abilities found in learned groups
            foreach (var learnedAbilityGroup in _learnedAbilityGroups)
            {
                AddLearnedAbilitiesFromLearnedAbilityGroup(learnedAbilityGroup.AbilityGroupDefinition);
            }

            player.Send("Please pick a weapon from the following choices:");
            DisplayWeaponList(player, false);
            return AvatarCreationStates.WeaponChoice;
        }
        var tokens = CommandParser.SplitParameters(input).ToArray();
        var command = tokens[0];
        var parameter = tokens.Length > 1
            ? string.Join(" ", tokens.Skip(1))
            : string.Empty;
        // from here, we will stay in state Customize
        if (command == "list")
        {
            DisplayCustomizeList(player);
        }
        else if (command == "learned")
        {
            DisplayCustomizeChosen(player);
        }
        else if (command == "add")
        {
            CustomizeAddAbilityGroupOrAbility(player, parameter);
        }
        else if (command == "drop")
        {
            CustomizeDropAbilityGroupOrAbility(player, parameter);
        }
        else if (command == "premise")
        {
            player.Send(
@"The ROM skill system allows you to fully customize your new character, making
him or her skilled in the skills you choose.  But beware, the skills you pick
at character creation are the only skills you will ever learn.  Skills are 
paid for with creation points, and the more creation points you have, the
harder it is to gain a level.  Furthermore, higher-cost skills are harder to
practice.

Skill groups are like package deals for characters -- sets of skills or spells
that are closely related, and hence can be learned as a unit.  There is a 
default skill group for each class, which can be selected for a balanced 
selection of skills at a somewhat reduced cost.

The experience breakdown is as follows: 
points   exp/level     points   exp/level
40        1000         90        6000
50        1500         100       8000
60	  2000         110      12000
70        3000         120      16000 
80        4000         130      24000
The table continues in a similar manner for higher point totals.");
        }
        else if (command == "help")
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                player.Send(
@"The following commands are available:
list         display all groups and skills not yet bought
learned      show all groups and skills bought 
premise      brief explanation of creation points and skill groups
add <name>   buy a skill or group
drop <name>  discard a skill or group
info <name>  list the skills or spells contained within a group
help <name>  help on skills and groups, or other help topics
done	     exit the character generation process");
            }
            else
            {
                parameter = "'" + parameter + "'";
                if (player is IAdmin admin)
                {
                    GameActionManager.Execute<Actor.Help, IAdmin>(admin, parameter);
                }
                else
                {
                    GameActionManager.Execute<Actor.Help, IPlayer>(player, parameter);
                }
            }
        }
        else if (command == "info" || command == "groups")
        {
            // display learned groups if not parameter
            if (string.IsNullOrWhiteSpace(parameter))
            {
                var sb = TableGenerators.AbilityGroupUsageTableGenerator.Value.Generate("Learned groups", _learnedAbilityGroups.OrderBy(x => x.Name));
                player.Page(sb);
            }
            // 
            else
            {
                // search ability group
                var abilityGroupDefinition = AbilityGroupManager.Search(CommandParser.ParseParameter(parameter));
                if (abilityGroupDefinition == null)
                    player.Send("No group of that name exist.");
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"%W%{abilityGroupDefinition.Name.ToPascalCase()}%x%");
                    if (abilityGroupDefinition.Help != null)
                    {
                        sb.AppendLine(abilityGroupDefinition.Help);
                    }
                    if (abilityGroupDefinition.AbilityDefinitions.Any())
                    {
                        var abilities = TableGenerators.AbilityDefinitionTableGenerator.Value.Generate("Abilities", true, abilityGroupDefinition.AbilityDefinitions);
                        sb.Append(abilities);
                    }
                    if (abilityGroupDefinition.AbilityGroupDefinitions.Any())
                    {
                        var subAbilityGroups = TableGenerators.AbilityGroupDefinitionTableGenerator.Value.Generate("Groups", true, abilityGroupDefinition.AbilityGroupDefinitions);
                        sb.Append(subAbilityGroups);
                    }
                    player.Page(sb);
                }
            }
        }

        // display choices
        player.Send("Choice (add,drop,list,learned,premise,help,info,done)");
        return AvatarCreationStates.Customize;
    }

    private AvatarCreationStates ProcessWeaponChoice(IPlayer player, string input)
    {
        if (input == "quit")
        {
            player.Send("Creation cancelled.");
            return AvatarCreationStates.Quit;
        }

        // search weapon matching input
        var matchingWeaponAbilityUsages = _learnedAbilities.Where(x => x.AbilityDefinition.Type == AbilityTypes.Weapon && StringCompareHelpers.StringStartsWith(x.Name, input)).ToList();
        if (matchingWeaponAbilityUsages.Count == 1)
        {
            // set default weapon to 40%
            var matchingWeaponAbilityUsage = matchingWeaponAbilityUsages.Single();
            var weaponAbilityUsage = _learnedAbilities.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, matchingWeaponAbilityUsage.Name));
            var learnedAbilityDatas = new List<LearnedAbilityData>();
            if (weaponAbilityUsage == null)
                Logger.LogError("AvatarCreationStateMachine: no matching ability usage found on class {className} for weapon {weaponAbilityName}", _class!.Name, matchingWeaponAbilityUsage.Name);
            else
            {
                var weaponLearnedAbilityData = new LearnedAbilityData
                {
                    Name = weaponAbilityUsage.Name,
                    ResourceKind = weaponAbilityUsage.ResourceKind,
                    CostAmount = weaponAbilityUsage.CostAmount,
                    CostAmountOperator = weaponAbilityUsage.CostAmountOperator,
                    Level = weaponAbilityUsage.Level,
                    Learned = Math.Max(weaponAbilityUsage.MinLearned, 40),
                    Rating = weaponAbilityUsage.Rating,
                };
                learnedAbilityDatas.Add(weaponLearnedAbilityData);
            }
            // map learned abilities to learned ability data
            foreach (var learnedAbility in _learnedAbilities)
            {
                var existingLearnedAbilityData = learnedAbilityDatas.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, learnedAbility.Name));
                if (existingLearnedAbilityData != null)
                {
                    existingLearnedAbilityData.CostAmount = Math.Min(existingLearnedAbilityData.CostAmount, learnedAbility.CostAmount);
                    existingLearnedAbilityData.Level = Math.Min(existingLearnedAbilityData.Level, learnedAbility.Level);
                    existingLearnedAbilityData.Learned = Math.Max(existingLearnedAbilityData.Learned, learnedAbility.MinLearned);
                    existingLearnedAbilityData.Rating = Math.Max(existingLearnedAbilityData.Rating, learnedAbility.Rating);
                }
                else
                {
                    var learnedAbilityData = new LearnedAbilityData
                    {
                        Name = learnedAbility.Name,
                        ResourceKind = learnedAbility.ResourceKind,
                        CostAmount = learnedAbility.CostAmount,
                        CostAmountOperator = learnedAbility.CostAmountOperator,
                        Level = learnedAbility.Level,
                        Learned = learnedAbility.Level <= 1
                            ? Math.Max(1, learnedAbility.MinLearned)
                            : Math.Max(0, learnedAbility.MinLearned),
                        Rating = learnedAbility.Rating,
                    };
                    learnedAbilityDatas.Add(learnedAbilityData);
                }
            }
            // map learned ability groups to learned ability group data
            var learnAbilityGroupDatas = new List<LearnedAbilityGroupData>();
            foreach (var learnedAbilityGroup in _learnedAbilityGroups)
            {
                var existingLearnedAbilityGroupData = learnAbilityGroupDatas.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, learnedAbilityGroup.Name));
                if (existingLearnedAbilityGroupData != null)
                {
                    existingLearnedAbilityGroupData.Cost = Math.Min(existingLearnedAbilityGroupData.Cost, learnedAbilityGroup.Cost);
                }
                else
                {
                    var learnedAbilityGroupData = new LearnedAbilityGroupData
                    {
                        Name = learnedAbilityGroup.Name,
                        Cost = learnedAbilityGroup.Cost
                    };
                    learnAbilityGroupDatas.Add(learnedAbilityGroupData);
                }
            }

            // known abilities will be generated in PlayableCharacter ctor when avatar will be impersonated

            var startingRoom = RoomManager.MudSchoolRoom;
            PlayableCharacterData playableCharacterData = new()
            {
                CreationTime = TimeManager.CurrentTime,
                Name = _name!,
                Aliases = [],
                AutoFlags = AutoFlags.None,
                RoomId = startingRoom?.Blueprint.Id ?? 3001, // TODO:  mud school
                Race = _race!.Name,
                Class = _class!.Name,
                Level = 1,
                Sex = _sex!.Value,
                Size = _race!.Size,
                HitPoints = 20,
                MovePoints = 100,
                CurrentResources = new Dictionary<ResourceKinds, int>
                {
                    { ResourceKinds.Mana, 100 },
                    { ResourceKinds.Psy, 100 },
                    // TODO: other resource
                },
                MaxResources = new Dictionary<ResourceKinds, int>
                {
                    { ResourceKinds.Mana, 100 },
                    { ResourceKinds.Psy, 100 },
                    // TODO: other resource
                },
                Experience = 0,
                Alignment = 0, // TODO
                Trains = 3,
                Practices = 5,
                GoldCoins = 0,
                SilverCoins = 0,
                Conditions = Enum.GetValues<Conditions>().Where(x => x != Conditions.Drunk).ToDictionary(x => x, x => 48),
                Equipments = [],
                Inventory = [],
                CurrentQuests = [],
                Auras = [],
                CharacterFlags = _race!.CharacterFlags,
                Immunities = _race!.Immunities,
                Resistances = _race!.Resistances,
                Vulnerabilities = _race!.Vulnerabilities,
                ShieldFlags = ShieldFlagFactory.CreateInstance(),
                Attributes = GetStartAttributeValues(_race!, _class!),
                LearnedAbilities = learnedAbilityDatas.ToArray(),
                LearnedAbilityGroups = learnAbilityGroupDatas.ToArray(),
                Cooldowns = [],
                Pets = []
            };

            player.AddAvatar(playableCharacterData);
            ServerPlayerCommand.Save(player);
            UniquenessManager.AddAvatarName(_name!);
            // TODO: better wording
            player.Send("Your avatar is created. Name: {0} Sex: {1} Race: {2} Class: {3}.", _name!.UpperFirstLetter(), _sex!, _race!.DisplayName, _class!.DisplayName);
            player.Send("Would you like to impersonate it now? (y/n)");
            return AvatarCreationStates.ImmediateImpersonate;
        }
        DisplayWeaponList(player, false);
        return AvatarCreationStates.WeaponChoice;
    }

    private AvatarCreationStates ProcessImmediateImpersonate(IPlayer player, string input)
    {
        player.ChangePlayerState(PlayerStates.Playing);
        if (input == "y" || input == "yes")
        {
            // Impersonate
            State = AvatarCreationStates.CreationComplete;
            string? executeResult;
            if (player is IAdmin admin)
            {
                executeResult = GameActionManager.Execute<Admin.Avatar.Impersonate, IAdmin>(admin, _name);
            }
            else
            {
                executeResult = GameActionManager.Execute<Avatar.Impersonate, IPlayer>(player, _name);
            }
            if (executeResult != null)
            {
                // should not happen
                player.Send("An error occurred while trying to impersonate your avatar.");
                return AvatarCreationStates.CreationComplete;
            }
            else
                return AvatarCreationStates.CreationComplete;
        }
        player.Send("Avatar {0} created but not impersonated. Use /impersonate {0} to enter game or use /list to see your avatar list.", _name!.UpperFirstLetter());
        // else, NOP
        return AvatarCreationStates.CreationComplete;
    }

    private AvatarCreationStates ProcessCreationComplete(IPlayer player, string input)
    {
        player.ChangePlayerState(PlayerStates.Playing);
        // fall-thru
        return AvatarCreationStates.CreationComplete;
    }

    private AvatarCreationStates ProcessQuit(IPlayer player, string input)
    {
        player.ChangePlayerState(PlayerStates.Playing);
        // fall-thru
        return AvatarCreationStates.Quit;
    }

    private void DisplaySexList(IPlayer player, bool displayHeader = true)
    {
        if (displayHeader)
            player.Send("Please choose a sex (type quit to stop creation).");
        string sexes = string.Join(" | ", Enum.GetNames(typeof(Sex)));
        player.Send(sexes);
    }

    private void DisplayRaceList(IPlayer player, bool displayHeader = true)
    {
        if (displayHeader)
            player.Send("Please choose a race (type quit to stop creation).");
        string races = string.Join(" | ", RaceManager.PlayableRaces.Select(x => x.DisplayName));
        player.Send(races);
    }

    private void DisplayClassList(IPlayer player, bool displayHeader = true)
    {
        if (displayHeader)
            player.Send("Please choose a class (type quit to stop creation).");
        string classes = string.Join(" | ", ClassManager.Classes.Select(x => x.DisplayName));
        player.Send(classes);
    }

    private void DisplayWeaponList(IPlayer player, bool displayHeader = true)
    {
        if (displayHeader)
            player.Send("Please choose a weapon (type quit to stop creation).");
        var weapons = string.Join(" | ", _learnedAbilities.Where(x => x.AbilityDefinition.Type == AbilityTypes.Weapon).Select(x => x.Name));
        player.Send(weapons);
    }

    private void DisplayCustomizeList(IPlayer player)
    {
        // display available groups with cost
        var availableAbilityGroupsNotYetLearned = GetAvailableAbilityGroupsNotYetLearnedNotChosen();
        var sbGroups = TableGenerators.AbilityGroupUsageTableGenerator.Value.Generate("Available groups", 3, availableAbilityGroupsNotYetLearned.OrderBy(x => x.Name));
        player.Send(sbGroups);
        // display available skills
        var availableSkillsNotYetLearned = GetAvailableSkillsNotYetLearnedNorChosen();
        var sbSkills = TableGenerators.AbilityUsageTableGenerator.Value.Generate("Available skills", 3, availableSkillsNotYetLearned.OrderBy(x => x.Name));
        player.Send(sbSkills);
        // display creation points
        player.Send("Creation points: {0}", _creationPoints);
        // TODO: display exp/level
    }

    private void DisplayCustomizeChosen(IPlayer player)
    {
        var sbGroups = TableGenerators.AbilityGroupUsageTableGenerator.Value.Generate("Chosen groups", 3, _chosenAbilityGroups.OrderBy(x => x.Name));
        player.Send(sbGroups);
        var sbSkills = TableGenerators.AbilityUsageTableGenerator.Value.Generate("Chosen skills", 3, _chosenAbilities.OrderBy(x => x.Name));
        player.Send(sbSkills);
    }

    private void CustomizeAddAbilityGroupOrAbility(IPlayer player, string name)
    {
        // search in groups
        var abilityGroupUsage = GetAvailableAbilityGroupsNotYetLearnedNotChosen().FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, name));
        if (abilityGroupUsage != null)
        {
            _chosenAbilityGroups.Add(abilityGroupUsage);
            _creationPoints += abilityGroupUsage.Cost;
            player.Send("{0} group added", abilityGroupUsage.Name.ToPascalCase());
            return;
        }
        // search in skills
        var skillUsage = GetAvailableSkillsNotYetLearnedNorChosen().FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, name));
        if (skillUsage != null)
        {
            _chosenAbilities.Add(skillUsage);
            _creationPoints += skillUsage.Rating;
            player.Send("{0} skill added", skillUsage.Name.ToPascalCase());
            return;
        }
        player.Send("No skills or groups by that name...");
    }

    private void CustomizeDropAbilityGroupOrAbility(IPlayer player, string name)
    {
        // search in chosen groups
        var abilityGroupUsage = _chosenAbilityGroups.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, name));
        if (abilityGroupUsage != null)
        {
            _chosenAbilityGroups.Remove(abilityGroupUsage);
            _creationPoints -= abilityGroupUsage.Cost;
            player.Send("{0} group dropped", abilityGroupUsage.Name.ToPascalCase());
            return;
        }
        // search in chosen skills
        var skillUsage = _chosenAbilities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, name));
        if (skillUsage != null)
        {
            _chosenAbilities.Remove(skillUsage);
            _creationPoints -= skillUsage.Rating;
            player.Send("{0} skill dropped", skillUsage.Name.ToPascalCase());
            return;
        }
        player.Send("You haven't bought any such skill or group.");
    }

    private Dictionary<CharacterAttributes, int> GetStartAttributeValues(IPlayableRace race, IClass @class)
        => Enum.GetValues<CharacterAttributes>().ToDictionary(x => x, x => GetStartAttributeValue(x, race, @class));

    private int GetStartAttributeValue(CharacterAttributes characterAttribute, IPlayableRace race, IClass @class)
    {
        switch (characterAttribute)
        {
            case CharacterAttributes.Strength:
            case CharacterAttributes.Intelligence:
            case CharacterAttributes.Wisdom:
            case CharacterAttributes.Dexterity:
            case CharacterAttributes.Constitution:
                {
                    var basicAttribute = (BasicAttributes)characterAttribute;
                    var value = race.GetStartAttribute(basicAttribute);
                    if (basicAttribute == @class.PrimeAttribute)
                        value += 3;
                    return value;
                }
            case CharacterAttributes.MaxHitPoints: return 100;
            case CharacterAttributes.SavingThrow: return 0;
            case CharacterAttributes.HitRoll: return 0;
            case CharacterAttributes.DamRoll: return 0;
            case CharacterAttributes.MaxMovePoints: return 100;
            case CharacterAttributes.ArmorBash: return 0;
            case CharacterAttributes.ArmorPierce: return 0;
            case CharacterAttributes.ArmorSlash: return 0;
            case CharacterAttributes.ArmorExotic: return 0;
            default:
                Logger.LogError("Unexpected character attribute {characterAttribute} during avatar creation", characterAttribute);
                return 0;
        }
    }

    private void AddLearnedAbilitiesFromLearnedAbilityGroup(IAbilityGroupDefinition abilityGroupDefinition)
    {
        foreach (var abilityDefinition in abilityGroupDefinition.AbilityDefinitions)
        {
            if (_learnedAbilities.Any(x => StringCompareHelpers.StringEquals(x.AbilityDefinition.Name, abilityDefinition.Name)))
                Logger.LogWarning("Ability {abillityName} defined in ability group {abilityGroupName} is defined multiple times in ability groups for class {className}", abilityDefinition.Name, abilityGroupDefinition.Name, _class!.Name);
            else
            {
                var abilityUsage = _class!.AvailableAbilities.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.AbilityDefinition.Name, abilityDefinition.Name));
                if (abilityUsage == null)
                    Logger.LogError("Ability {abillityName} defined in ability group {abilityGroupName} is not available for class {className}", abilityDefinition.Name, abilityGroupDefinition.Name, _class!.Name);
                else
                    _learnedAbilities.Add(abilityUsage);
            }
        }
        foreach (var subGroup in abilityGroupDefinition.AbilityGroupDefinitions)
            AddLearnedAbilitiesFromLearnedAbilityGroup(subGroup);
    }

    private IEnumerable<IAbilityGroupUsage> GetAvailableAbilityGroupsNotYetLearnedNotChosen()
        => _class!.AvailableAbilityGroups.Where(x => 
            _chosenAbilityGroups.All(y => !StringCompareHelpers.StringEquals(y.Name, x.Name))
            && _learnedAbilityGroups.All(y => !StringCompareHelpers.StringEquals(y.Name, x.Name)));

    private IEnumerable<IAbilityUsage> GetAvailableSkillsNotYetLearnedNorChosen()
        => _class!.AvailableAbilities.Where(x =>
            (x.AbilityDefinition.Type == AbilityTypes.Skill || x.AbilityDefinition.Type == AbilityTypes.Passive || x.AbilityDefinition.Type == AbilityTypes.Weapon)
            && _chosenAbilities.All(y => !StringCompareHelpers.StringEquals(y.Name, x.Name))
            && _learnedAbilities.All(y => !StringCompareHelpers.StringEquals(y.Name, x.Name)));
}
