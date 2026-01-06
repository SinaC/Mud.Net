using Microsoft.Extensions.Logging;
using Moq;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Blueprints.Character;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Server.Character.PlayableCharacter;
using Mud.Server.Flags;
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
using Mud.Server.Random;

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
            var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
            var roomManagerMock = new Mock<IRoomManager>();
            var itemManagerMock = new Mock<IItemManager>();
            var characterManagerMock = new Mock<ICharacterManager>();
            var classManagerMock = new Mock<IClassManager>();
            var raceManagerMock = new Mock<IRaceManager>();
            var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
            var wiznetMock = new Mock<IWiznet>();

            var playerMock = new Mock<IPlayer>();
            var roomMock = new Mock<IRoom>();

            classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
            raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IPlayableRace>().Object);

            var pcData = new AvatarData
            {
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
                CurrentQuests = [],
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

            var pc = new PlayableCharacter(loggerMock.Object, null!, null!, null!, messageForwardOptions, worldOptions, null!, null!, roomManagerMock.Object, itemManagerMock.Object, characterManagerMock.Object, null!, null!, null!, wiznetMock.Object, raceManagerMock.Object, classManagerMock.Object, null!, resistanceCalculatorMock.Object, null!, null!, null!);
            pc.Initialize(Guid.NewGuid(), pcData, playerMock.Object, roomMock.Object);
            pc.AbilityDamage(pc, 100000, SchoolTypes.Poison, "poison", false);

            playerMock.Verify(x => x.Send("You have been KILLED!!", It.IsAny<bool>()), Times.Once);
            itemManagerMock.Verify(x => x.AddItemCorpse(It.IsAny<Guid>(), It.IsAny<IRoom>(), pc, pc), Times.Once);
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

            var blueprint = GenerateBluePrint();

            classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
            raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IRace>().Object);

            var npc = new NonPlayableCharacter(loggerMock.Object, null!, null!, null!, messageForwardOptions, randomManagerMock.Object, tableValuesMock.Object, null!, itemManagerMock.Object, characterManagerMock.Object, null!, null!, wiznetMock.Object, raceManagerMock.Object, classManagerMock.Object, resistanceCalculatorMock.Object, null!, null!, null!, null!);
            npc.Initialize(Guid.NewGuid(), blueprint, roomMock.Object);
            npc.AbilityDamage(npc, 100000, SchoolTypes.Poison, "poison", false);

            loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Debug),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString()!.Contains("You have been KILLED!!")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            itemManagerMock.Verify(x => x.AddItemCorpse(It.IsAny<Guid>(), It.IsAny<IRoom>(), npc, npc), Times.Once);
        }

        private static CharacterNormalBlueprint GenerateBluePrint()
            => new()
            {
                Id = 1,
                Name = "mob1",
                ActFlags = new ActFlags("NoAlign", "Gain"),
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
