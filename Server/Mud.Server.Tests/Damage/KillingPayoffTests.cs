using Microsoft.Extensions.Logging;
using Moq;
using Mud.Blueprints.Character;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Server.Character.PlayableCharacter;
using Mud.Flags;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Options;
using Mud.Random;
using Mud.Server.Interfaces.Loot;
using System.ComponentModel.DataAnnotations;

namespace Mud.Server.Tests.Damage;

[TestClass]
public class KillingPayoffTests
{
    [TestMethod]
    public void PetPerformTheKill()
    {
        var pcLoggerMock = new Mock<ILogger<PlayableCharacter>>();
        var npcLoggerMock = new Mock<ILogger<NonPlayableCharacter>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = false, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var roomManagerMock = new Mock<IRoomManager>();
        var itemManagerMock = new Mock<IItemManager>();
        var characterManagerMock = new Mock<ICharacterManager>();
        var classManagerMock = new Mock<IClassManager>();
        var raceManagerMock = new Mock<IRaceManager>();
        var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
        var wiznetMock = new Mock<IWiznet>();
        var randomManagerMock = new Mock<IRandomManager>();
        var tableValuesMock = new Mock<ITableValues>();
        var lootManagerMock = new Mock<ILootManager>();
        var aggroManagerMock = new Mock<IAggroManager>();

        var playerMock = new Mock<IPlayer>();
        var roomMock = new Mock<IRoom>();

        var playableRaceMock = new Mock<IPlayableRace>();
        playableRaceMock.Setup(x => x.ClassExperiencePercentageMultiplier(It.IsAny<IClass>())).Returns(100);
        classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
        raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(playableRaceMock.Object);
        roomMock.SetupGet(x => x.RoomFlags).Returns(new RoomFlags());
        randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => (min + max) / 2);

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
            AutoFlags = AutoFlags.None,
            Currencies = [],
            ActiveQuests = [],
            LearnedAbilities = [],
            LearnedAbilityGroups = [],
            Conditions = [],
            Aliases = [],
            Cooldowns = [],
            Pets = [],
            // CharacterBaseData
            Name = "Player",
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

        var petBlueprint = new CharacterNormalBlueprint()
        {
            Id = 1,
            Name = "pet1",
            ActFlags = new ActFlags("NoAlign"),
            OffensiveFlags = new OffensiveFlags("AreaAttack", "Bash"),
            CharacterFlags = new CharacterFlags("Sanctuary", "Regeneration"),
            Immunities = new IRVFlags(),
            Resistances = new IRVFlags(),
            Vulnerabilities = new IRVFlags(),
            ShieldFlags = new ShieldFlags(),
            Level = 50,
            Sex = Sex.Neutral,
            StartPosition = Positions.Standing,
            DefaultPosition = Positions.Standing,
            HitPointDiceCount = 10,
            HitPointDiceValue = 5,
            HitPointDiceBonus = 23,
        };

        var victimBlueprint = new CharacterNormalBlueprint()
        {
            Id = 1,
            Name = "victim1",
            ActFlags = new ActFlags("NoAlign"),
            OffensiveFlags = new OffensiveFlags("AreaAttack", "Bash"),
            CharacterFlags = new CharacterFlags("Sanctuary", "Regeneration"),
            Immunities = new IRVFlags(),
            Resistances = new IRVFlags(),
            Vulnerabilities = new IRVFlags(),
            ShieldFlags = new ShieldFlags(),
            Level = 50,
            Sex = Sex.Neutral,
            StartPosition = Positions.Standing,
            DefaultPosition = Positions.Standing,
            HitPointDiceCount = 10,
            HitPointDiceValue = 5,
            HitPointDiceBonus = 23,
        };

        var pc = new PlayableCharacter(pcLoggerMock.Object, null!, null!, messageForwardOptions, worldOptions, null!, randomManagerMock.Object, tableValuesMock.Object, roomManagerMock.Object, itemManagerMock.Object, characterManagerMock.Object, null!, null!, null!, wiznetMock.Object, lootManagerMock.Object, aggroManagerMock.Object, raceManagerMock.Object, classManagerMock.Object, null!, resistanceCalculatorMock.Object, null!, null!, null!, null!);
        pc.Initialize(Guid.NewGuid(), pcData, playerMock.Object, roomMock.Object);
        var pet = new NonPlayableCharacter(npcLoggerMock.Object, null!, null!, messageForwardOptions, null!, randomManagerMock.Object, tableValuesMock.Object, null!, itemManagerMock.Object, characterManagerMock.Object, null!, null!, wiznetMock.Object, lootManagerMock.Object, aggroManagerMock.Object, raceManagerMock.Object, classManagerMock.Object, resistanceCalculatorMock.Object, null!, null!, null!, null!);
        pet.Initialize(Guid.NewGuid(), petBlueprint, roomMock.Object);
        pc.AddPet(pet);
        var victim = new NonPlayableCharacter(npcLoggerMock.Object, null!, null!, messageForwardOptions, null!, randomManagerMock.Object, tableValuesMock.Object, null!, itemManagerMock.Object, characterManagerMock.Object, null!, null!, wiznetMock.Object, lootManagerMock.Object, aggroManagerMock.Object, raceManagerMock.Object, classManagerMock.Object, resistanceCalculatorMock.Object, null!, null!, null!, null!);
        victim.Initialize(Guid.NewGuid(), victimBlueprint, roomMock.Object);

        victim.AbilityDamage(pet, 100000, SchoolTypes.Poison, "poison", false);

        // even when pet performs the kill, pet's master KillingPayoff must be called and corpse must be created using every players in the group
        // 2120 xp gained, 2 levels gained, 0 alignment modification (victim is NoAlign)
        Assert.AreEqual(2120, pc.Experience);
        Assert.AreEqual(3, pc.Level);
        Assert.AreEqual(0, pc.Alignment);
        itemManagerMock.Verify(x => x.AddItemCorpse(It.IsAny<Guid>(), victim, It.IsAny<string>(), roomMock.Object), Times.Once);
        lootManagerMock.Verify(x => x.GenerateLoots(It.IsAny<IItemCorpse?>(), victim, It.Is<IEnumerable<IPlayableCharacter>>(x => x.Contains(pc))), Times.Once);
    }

    [TestMethod]
    public void GroupMemberPerformTheKill()
    {
        var pcLoggerMock = new Mock<ILogger<PlayableCharacter>>();
        var npcLoggerMock = new Mock<ILogger<NonPlayableCharacter>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 100, UseAggro = false, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var roomManagerMock = new Mock<IRoomManager>();
        var itemManagerMock = new Mock<IItemManager>();
        var characterManagerMock = new Mock<ICharacterManager>();
        var classManagerMock = new Mock<IClassManager>();
        var raceManagerMock = new Mock<IRaceManager>();
        var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
        var wiznetMock = new Mock<IWiznet>();
        var randomManagerMock = new Mock<IRandomManager>();
        var tableValuesMock = new Mock<ITableValues>();
        var lootManagerMock = new Mock<ILootManager>();
        var aggroManagerMock = new Mock<IAggroManager>();

        var playerMock = new Mock<IPlayer>();
        var roomMock = new Mock<IRoom>();

        var playableRaceMock = new Mock<IPlayableRace>();
        playableRaceMock.Setup(x => x.ClassExperiencePercentageMultiplier(It.IsAny<IClass>())).Returns(100);
        classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
        raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(playableRaceMock.Object);
        roomMock.SetupGet(x => x.RoomFlags).Returns(new RoomFlags());
        randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => (min + max) / 2);

        var pcData1 = new AvatarData
        {
            Version = 1,
            AccountName = "Account",
            CreationTime = DateTime.Now,
            RoomId = 0,
            SilverCoins = 0,
            GoldCoins = 0,
            Wimpy = 0,
            Experience = 4900,
            Alignment = 0,
            Trains = 0,
            Practices = 0,
            AutoFlags = AutoFlags.None,
            Currencies = [],
            ActiveQuests = [],
            LearnedAbilities = [],
            LearnedAbilityGroups = [],
            Conditions = [],
            Aliases = [],
            Cooldowns = [],
            Pets = [],
            // CharacterBaseData
            Name = "Player1",
            Race = "Human",
            Class = "Warrior",
            Level = 5,
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

        var pcData2 = new AvatarData
        {
            Version = 1,
            AccountName = "Account",
            CreationTime = DateTime.Now,
            RoomId = 0,
            SilverCoins = 0,
            GoldCoins = 0,
            Wimpy = 0,
            Experience = 11900,
            Alignment = 0,
            Trains = 0,
            Practices = 0,
            AutoFlags = AutoFlags.None,
            Currencies = [],
            ActiveQuests = [],
            LearnedAbilities = [],
            LearnedAbilityGroups = [],
            Conditions = [],
            Aliases = [],
            Cooldowns = [],
            Pets = [],
            // CharacterBaseData
            Name = "Player2",
            Race = "Human",
            Class = "Warrior",
            Level = 12,
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

        var victimBlueprint = new CharacterNormalBlueprint()
        {
            Id = 1,
            Name = "victim1",
            ActFlags = new ActFlags("NoAlign"),
            OffensiveFlags = new OffensiveFlags("AreaAttack", "Bash"),
            CharacterFlags = new CharacterFlags("Sanctuary", "Regeneration"),
            Immunities = new IRVFlags(),
            Resistances = new IRVFlags(),
            Vulnerabilities = new IRVFlags(),
            ShieldFlags = new ShieldFlags(),
            Level = 50,
            Sex = Sex.Neutral,
            StartPosition = Positions.Standing,
            DefaultPosition = Positions.Standing,
            HitPointDiceCount = 10,
            HitPointDiceValue = 5,
            HitPointDiceBonus = 23,
        };

        var pc1 = new PlayableCharacter(pcLoggerMock.Object, null!, null!, messageForwardOptions, worldOptions, null!, randomManagerMock.Object, tableValuesMock.Object, roomManagerMock.Object, itemManagerMock.Object, characterManagerMock.Object, null!, null!, null!, wiznetMock.Object, lootManagerMock.Object, aggroManagerMock.Object, raceManagerMock.Object, classManagerMock.Object, null!, resistanceCalculatorMock.Object, null!, null!, null!, null!);
        pc1.Initialize(Guid.NewGuid(), pcData1, playerMock.Object, roomMock.Object);
        var pc2 = new PlayableCharacter(pcLoggerMock.Object, null!, null!, messageForwardOptions, worldOptions, null!, randomManagerMock.Object, tableValuesMock.Object, roomManagerMock.Object, itemManagerMock.Object, characterManagerMock.Object, null!, null!, null!, wiznetMock.Object, lootManagerMock.Object, aggroManagerMock.Object, raceManagerMock.Object, classManagerMock.Object, null!, resistanceCalculatorMock.Object, null!, null!, null!, null!);
        pc2.Initialize(Guid.NewGuid(), pcData2, playerMock.Object, roomMock.Object);
        var group = new Group.Group(pc1);
        group.AddMember(pc2);
        var victim = new NonPlayableCharacter(npcLoggerMock.Object, null!, null!, messageForwardOptions, null!, randomManagerMock.Object, tableValuesMock.Object, null!, itemManagerMock.Object, characterManagerMock.Object, null!, null!, wiznetMock.Object, lootManagerMock.Object, aggroManagerMock.Object, raceManagerMock.Object, classManagerMock.Object, resistanceCalculatorMock.Object, null!, null!, null!, null!);
        victim.Initialize(Guid.NewGuid(), victimBlueprint, roomMock.Object);

        victim.AbilityDamage(pc2, 100000, SchoolTypes.Poison, "poison", false);

        // pc1: 340 xp gained -> go next level
        Assert.AreEqual(4900 + 340, pc1.Experience);
        Assert.AreEqual(6, pc1.Level);
        Assert.AreEqual(0, pc1.Alignment);
        // pc2: 630 xp gained -> go next level
        Assert.AreEqual(11900 + 630, pc2.Experience);
        Assert.AreEqual(13, pc2.Level);
        Assert.AreEqual(0, pc2.Alignment);
        //
        itemManagerMock.Verify(x => x.AddItemCorpse(It.IsAny<Guid>(), victim, It.IsAny<string>(), roomMock.Object), Times.Once);
        lootManagerMock.Verify(x => x.GenerateLoots(It.IsAny<IItemCorpse?>(), victim, It.Is<IEnumerable<IPlayableCharacter>>(x => x.Contains(pc2))), Times.Once); // pc2 performed the kill
    }
}
