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
using System.Text.RegularExpressions;

namespace Mud.Server.Commands.Player;

public enum AvatarCreationStates
{
    NameChoice, // -> NameConfirmation | NameChoice | Quit
    NameConfirmation, // -> NameChoice | SexChoice | Quit
    SexChoice, // -> SexChoice | RaceChoice | Quit
    RaceChoice, // -> RaceChoice | ClassChoice | Quit
    ClassChoice, // -> ClassChoice | WeaponChoice
    WeaponChoice, // -> CreationComplete | ImmediateImpersonate | Quit
    ImmediateImpersonate, // -> CreationComplete
    CreationComplete,
    Quit
}

internal class AvatarCreationStateMachine : InputTrapBase<IPlayer, AvatarCreationStates>
{
    private string? _name;
    private Sex? _sex;
    private IPlayableRace? _race;
    private IClass? _class;
    private List<IAbilityUsage> _learnedAbilities = [];
    private List<IAbilityGroupUsage> _learnedAbilityGroups = [];

    private ILogger Logger { get; }
    protected IServerPlayerCommand ServerPlayerCommand { get; }
    protected IRaceManager RaceManager { get; }
    protected IClassManager ClassManager { get; }
    protected IUniquenessManager UniquenessManager { get; }
    protected ITimeManager TimeManager { get; }
    protected IRoomManager RoomManager { get; }
    protected IFlagFactory<IShieldFlags, IShieldFlagValues> ShieldFlagFactory { get; }
    protected IGameActionManager GameActionManager { get; }

    public override bool IsFinalStateReached => State == AvatarCreationStates.CreationComplete || State == AvatarCreationStates.Quit;

    public AvatarCreationStateMachine(ILogger logger, IServerPlayerCommand serverPlayerCommand, IRaceManager raceManager, IClassManager classManager, IUniquenessManager uniquenessManager, ITimeManager timeManager, IRoomManager roomManager, IFlagFactory<IShieldFlags, IShieldFlagValues> shieldFlagFactory, IGameActionManager gameActionManager)
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

        KeepInputAsIs = false;
        StateMachine = new Dictionary<AvatarCreationStates, Func<IPlayer, string, AvatarCreationStates>>
        {
            {AvatarCreationStates.NameChoice, ProcessNameChoice},
            {AvatarCreationStates.NameConfirmation, ProcessNameConfirmation},
            {AvatarCreationStates.SexChoice, ProcessSexChoice},
            {AvatarCreationStates.RaceChoice, ProcessRaceChoice},
            {AvatarCreationStates.ClassChoice, ProcessClassChoice},
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
            // TODO: customization
            // add default ability groups
            foreach (var defaultAbilityGroup in _class.DefaultAbilityGroups)
            {
                _learnedAbilityGroups.Add(defaultAbilityGroup);
            }
            // add abilities found in group
            foreach (var learnedAbilityGroup in _learnedAbilityGroups)
            {
                AddLearnedAbilitiesFromLearnedAbilityGroup(learnedAbilityGroup.AbilityGroupDefinition);
            }
            player.Send("Please pick a weapon from the following choices:");
            DisplayWeaponList(player, false);
            return AvatarCreationStates.WeaponChoice;

        }
        DisplayClassList(player);
        return AvatarCreationStates.ClassChoice;
    }

    private AvatarCreationStates ProcessWeaponChoice(IPlayer player, string input)
    {
        if (input == "quit")
        {
            player.Send("Creation cancelled.");
            return AvatarCreationStates.Quit;
        }

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

            // TODO: abilities+ability groups (aka customization)
            // additional known abilities will be added in PlayableCharacter ctor

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
                HitPoints = 100,
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
                Equipments = [], //TODO: Equipments
                Inventory = [], //TODO: Inventory
                CurrentQuests = [], //TODO: CurrentQuests
                Auras = [], //TODO: Auras
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
            if (player is IAdmin)
            {
                executeResult = GameActionManager.Execute<Admin.Avatar.Impersonate, IPlayer>(player, _name);
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
}
