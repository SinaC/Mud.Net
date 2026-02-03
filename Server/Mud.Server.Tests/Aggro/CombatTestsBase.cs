using Microsoft.Extensions.Logging;
using Moq;
using Mud.Blueprints.Character;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Server.Character.PlayableCharacter;
using Mud.Server.Class.Interfaces;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Server.Loot.Interfaces;
using Mud.Server.Options;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.Tests.Aggro;

public abstract class CombatTestsBase
{
    protected static INonPlayableCharacter GenerateNPC(string name, IRandomManager randomManager, IAggroManager aggroManager, IRoom room)
    {
        var loggerMock = new Mock<ILogger<NonPlayableCharacter>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
        var classManagerMock = new Mock<IClassManager>();
        var raceManagerMock = new Mock<IRaceManager>();
        var rageGeneratorMock = new Mock<IRageGenerator>();
        var flagsManagerMock = new Mock<IFlagsManager>();

        classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
        raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IRace>().Object);

        var blueprint = new CharacterNormalBlueprint
        {
            // CharacterBaseData
            Name = name,
            Race = "Human",
            Class = "Warrior",
            Level = 6,
            Sex = Sex.Male,
            Size = Sizes.Medium,
            HitPointDiceCount = 10,
            HitPointDiceValue = 15,
            HitPointDiceBonus = 100,
            CharacterFlags = new CharacterFlags(),
            Immunities = new IRVFlags(),
            Resistances = new IRVFlags(),
            Vulnerabilities = new IRVFlags(),
            ActFlags = new ActFlags(),
            OffensiveFlags = new OffensiveFlags(),
            ShieldFlags = new ShieldFlags(),
            DefaultPosition = Positions.Standing,
            StartPosition = Positions.Standing,
        };

        var npc = new NonPlayableCharacter(loggerMock.Object, null!, null!, messageForwardOptions, null!, randomManager, null!, null!, null!, null!, null!, null!, null!, null!, aggroManager, raceManagerMock.Object, classManagerMock.Object, resistanceCalculatorMock.Object, rageGeneratorMock.Object, null!, flagsManagerMock.Object, null!);
        npc.Initialize(Guid.NewGuid(), blueprint, room);

        return npc;
    }

    protected static IPlayableCharacter GeneratePC(string name, IAggroManager aggroManager, IRoom room)
    {
        var loggerMock = new Mock<ILogger<PlayableCharacter>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var roomManagerMock = new Mock<IRoomManager>();
        var itemManagerMock = new Mock<IItemManager>();
        var characterManagerMock = new Mock<ICharacterManager>();
        var classManagerMock = new Mock<IClassManager>();
        var raceManagerMock = new Mock<IRaceManager>();
        var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
        var wiznetMock = new Mock<IWiznet>();
        var lootManagerMock = new Mock<ILootManager>();
        var rageGeneratorMock = new Mock<IRageGenerator>();
        var flagsManagerMock = new Mock<IFlagsManager>();

        var playerMock = new Mock<IPlayer>();

        classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
        raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IPlayableRace>().Object);

        var pcData = new AvatarData
        {
            Version = 1,
            AccountName = "Account",
            CreationTime = DateTime.Now,
            RoomId = 0,
            SilverCoins = 0,
            GoldCoins = 0,
            Wimpy = 0,
            Experience = 0,
            Alignment = 0,
            Trains = 0,
            Practices = 0,
            AutoFlags = null!,
            Currencies = [],
            ActiveQuests = [],
            LearnedAbilities = [],
            LearnedAbilityGroups = [],
            Conditions = [],
            Aliases = [],
            Cooldowns = [],
            Pets = [],
            // CharacterBaseData
            Name = name,
            Race = "Human",
            Class = "Warrior",
            Level = 1,
            Sex = Sex.Male,
            Size = Sizes.Medium,
            CurrentResources = new Dictionary<ResourceKinds, int>
                {
                    { ResourceKinds.HitPoints, 1000 },
                },
            MaxResources = new Dictionary<ResourceKinds, int>
                {
                    { ResourceKinds.HitPoints, 1000 },
                },
            Equipments = [],
            Inventory = [],
            Auras = [], // TODO: add poison and trigger poison effect
            CharacterFlags = string.Empty,
            Immunities = string.Empty,
            Resistances = string.Empty,
            Vulnerabilities = string.Empty,
            ShieldFlags = string.Empty,
            Attributes = [],
        };

        var pc = new PlayableCharacter(loggerMock.Object, null!, null!, messageForwardOptions, worldOptions, null!, null!, null!, roomManagerMock.Object, itemManagerMock.Object, characterManagerMock.Object, null!, null!, flagsManagerMock.Object, wiznetMock.Object, lootManagerMock.Object, aggroManager, raceManagerMock.Object, classManagerMock.Object, null!, resistanceCalculatorMock.Object, rageGeneratorMock.Object, null!, null!, null!);
        pc.Initialize(Guid.NewGuid(), pcData, playerMock.Object, room);

        return pc;
    }
}
