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
using Mud.Server.Interfaces.Table;
using Mud.Server.Loot.Interfaces;
using Mud.Server.Options;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.Tests.PlayableCharacters
{
    [TestClass]
    public class SelfDeathDamageTests
    {
        [TestMethod]
        public void PC()
        {
            var loggerMock = new Mock<ILogger<PlayableCharacter>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = false, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
            var roomManagerMock = new Mock<IRoomManager>();
            var itemManagerMock = new Mock<IItemManager>();
            var characterManagerMock = new Mock<ICharacterManager>();
            var classManagerMock = new Mock<IClassManager>();
            var raceManagerMock = new Mock<IRaceManager>();
            var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
            var wiznetMock = new Mock<IWiznet>();
            var lootManagerMock = new Mock<ILootManager>();
            var aggroManagerMock = new Mock<IAggroManager>();
            var flagsManagerMock = new Mock<IFlagsManager>();

            var playerMock = new Mock<IPlayer>();
            var roomMock = new Mock<IRoom>();

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

            var pc = new PlayableCharacter(loggerMock.Object, null!, null!, messageForwardOptions, worldOptions, null!, null!, null!, roomManagerMock.Object, itemManagerMock.Object, characterManagerMock.Object, null!, null!, flagsManagerMock.Object, wiznetMock.Object, lootManagerMock.Object, aggroManagerMock.Object, raceManagerMock.Object, classManagerMock.Object, null!, resistanceCalculatorMock.Object, null!, null!, null!, null!);
            pc.Initialize(Guid.NewGuid(), pcData, playerMock.Object, roomMock.Object);
            pc.AbilityDamage(pc, 100000, SchoolTypes.Poison, "poison", false);

            playerMock.Verify(x => x.Send("%R%You have been KILLED!!%x%", It.IsAny<bool>()), Times.Once);
            itemManagerMock.Verify(x => x.AddItemCorpse(It.IsAny<Guid>(), pc, It.IsAny<string>(), It.IsAny<IRoom>()), Times.Once);
            lootManagerMock.Verify(x => x.GenerateLoots(It.IsAny<IItemCorpse?>(), pc, It.Is<IEnumerable<IPlayableCharacter>>(x => x.Contains(pc))), Times.Once);
        }

        [TestMethod]
        public void NPC()
        {
            var loggerMock = new Mock<ILogger<NonPlayableCharacter>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var classManagerMock = new Mock<IClassManager>();
            var raceManagerMock = new Mock<IRaceManager>();
            var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
            var randomManagerMock = new Mock<IRandomManager>();
            var tableValuesMock = new Mock<ITableValues>();
            var characterManagerMock = new Mock<ICharacterManager>();
            var itemManagerMock = new Mock<IItemManager>();
            var wiznetMock = new Mock<IWiznet>();
            var roomMock = new Mock<IRoom>();
            var lootManagerMock = new Mock<ILootManager>();
            var aggroManagerMock = new Mock<IAggroManager>();
            var flagsManagerMock = new Mock<IFlagsManager>();

            var blueprint = GenerateBluePrint();

            classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
            raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IRace>().Object);

            var npc = new NonPlayableCharacter(loggerMock.Object, null!, null!, messageForwardOptions, null!, randomManagerMock.Object, tableValuesMock.Object, null!, itemManagerMock.Object, characterManagerMock.Object, null!, null!, wiznetMock.Object, lootManagerMock.Object, aggroManagerMock.Object, raceManagerMock.Object, classManagerMock.Object, resistanceCalculatorMock.Object, null!, null!, flagsManagerMock.Object, null!);
            npc.Initialize(Guid.NewGuid(), blueprint, roomMock.Object);
            npc.AbilityDamage(npc, 100000, SchoolTypes.Poison, "poison", false);

            loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Debug),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString()!.Contains("%R%You have been KILLED!!%x%")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            itemManagerMock.Verify(x => x.AddItemCorpse(It.IsAny<Guid>(), npc, It.IsAny<string>(), It.IsAny<IRoom>()), Times.Once);
            lootManagerMock.Verify(x => x.GenerateLoots(It.IsAny<IItemCorpse?>(), npc, It.Is<IEnumerable<IPlayableCharacter>>(x => x.Count() == 0)), Times.Once);
        }

        private static CharacterNormalBlueprint GenerateBluePrint()
            => new()
            {
                Id = 1,
                Name = "mob1",
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
    }
}
