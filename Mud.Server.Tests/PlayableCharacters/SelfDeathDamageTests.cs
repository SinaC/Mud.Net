using Microsoft.Extensions.Logging;
using Moq;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Character.PlayableCharacter;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Tests.PlayableCharacters
{
    [TestClass]
    public class SelfDeathDamageTests
    {
        [TestMethod]
        public void SelfDeathFromPoison()
        {
            var loggerMock = new Mock<ILogger<PlayableCharacter>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
            var roomManagerMock = new Mock<IRoomManager>();
            var itemManagerMock = new Mock<IItemManager>();
            var characterManagerMock = new Mock<ICharacterManager>();
            var classManagerMock = new Mock<IClassManager>();
            var raceManagerMock = new Mock<IRaceManager>();
            var damageModifierManagerMock = new Mock<IDamageModifierManager>();
            var wiznetMock = new Mock<IWiznet>();

            var playerMock = new Mock<IPlayer>();
            var roomMock = new Mock<IRoom>();

            classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
            raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IRace>().Object);

            var pcData = new PlayableCharacterData
            {
                // CharacterBaseData
                Name = "Player",
                Race = "Human",
                Class = "Warrior",
                Level = 1,
                Sex = Sex.Male,
                Size = Sizes.Medium,
                HitPoints = 1,
                MovePoints = 1,
                CurrentResources = [],
                MaxResources = [],
                Equipments = [],
                Inventory = [],
                Auras = [], // TODO: add poison and trigger poison effect
                CharacterFlags = string.Empty,
                Immunities = string.Empty,
                Resistances = string.Empty,
                Vulnerabilities = string.Empty,
                ShieldFlags = string.Empty,
                Attributes = [],
                // PlayableCharacterData
                CreationTime = DateTime.Now,
                RoomId = 0,
                SilverCoins = 0,
                GoldCoins = 0,
                Experience = 0,
                Alignment = 0,
                Trains = 0,
                Practices = 0,
                AutoFlags = AutoFlags.None,
                CurrentQuests = [],
                LearnedAbilities = [],
                LearnedAbilityGroups = [],
                Conditions = [],
                Aliases = [],
                Cooldowns = [],
                Pets = []
            };

            var pc = new PlayableCharacter(loggerMock.Object, null!, null!, null!, messageForwardOptions, worldOptions, null!, null!, roomManagerMock.Object, itemManagerMock.Object, characterManagerMock.Object, null!, null!, wiznetMock.Object, raceManagerMock.Object, classManagerMock.Object, null!, damageModifierManagerMock.Object, null!, null!, null!);
            pc.Initialize(Guid.NewGuid(), pcData, playerMock.Object, roomMock.Object);
            pc.AbilityDamage(pc, 100000, SchoolTypes.Poison, "poison", false);

            playerMock.Verify(x => x.Send("You have been KILLED!!", It.IsAny<bool>()), Times.Once);
            itemManagerMock.Verify(x => x.AddItemCorpse(It.IsAny<Guid>(), It.IsAny<IRoom>(), pc, pc), Times.Once);
        }
    }
}
