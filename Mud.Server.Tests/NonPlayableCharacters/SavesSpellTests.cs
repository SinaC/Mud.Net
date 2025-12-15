using Microsoft.Extensions.Logging;
using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Server.Random;

namespace Mud.Server.Tests.NonPlayableCharacters
{
    [TestClass]
    public class SavesSpellTests : TestBase
    {
        [TestMethod]
        public void Npc_ChanceAlwaysSuccess()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true);
            var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
            var victim = GenerateNPC(randomManagerMock.Object, resistanceCalculatorMock.Object, "", "", "", "");

            bool savesSpell = victim.SavesSpell(10, SchoolTypes.Acid);

            Assert.IsTrue(savesSpell);
            randomManagerMock.Verify(x => x.Chance(30), Times.Once);
        }

        [TestMethod]
        public void Npc_ChanceAlwaysFail()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false);
            var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
            var victim = GenerateNPC(randomManagerMock.Object, resistanceCalculatorMock.Object, "", "", "", "");

            bool savesSpell = victim.SavesSpell(10, SchoolTypes.Acid);

            Assert.IsFalse(savesSpell);
            randomManagerMock.Verify(x => x.Chance(30), Times.Once);
        }


        [TestMethod]
        [DataRow("", "", "", "", SchoolTypes.Acid, 30)]
        [DataRow("Acid", "", "", "", SchoolTypes.Acid, null)]
        [DataRow("", "Acid", "", "", SchoolTypes.Acid, 32)]
        [DataRow("", "", "Acid", "", SchoolTypes.Acid, 28)]
        [DataRow("Acid", "", "", "", SchoolTypes.Fire, 30)]
        [DataRow("", "Acid", "", "", SchoolTypes.Fire, 30)]
        [DataRow("", "", "Acid", "", SchoolTypes.Fire, 30)]
        [DataRow("", "", "", "Berserk", SchoolTypes.Acid, 33)]
        [DataRow("Acid", "", "", "Berserk", SchoolTypes.Acid, null)]
        [DataRow("", "Acid", "", "Berserk", SchoolTypes.Acid, 35)]
        [DataRow("", "", "Acid", "Berserk", SchoolTypes.Acid, 31)]
        [DataRow("Acid", "", "", "Berserk", SchoolTypes.Fire, 33)]
        [DataRow("", "Acid", "", "Berserk", SchoolTypes.Fire, 33)]
        [DataRow("", "", "Acid", "Berserk", SchoolTypes.Fire, 33)]
        public void Npc(string imm, string res, string vuln, string flags, SchoolTypes damType, int? expected)
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true);
            var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
            resistanceCalculatorMock.Setup(x => x.CheckResistance(It.IsAny<ICharacter>(), It.IsAny<SchoolTypes>())).Returns<ICharacter, SchoolTypes>(CheckResistances);
            var victim = GenerateNPC(randomManagerMock.Object, resistanceCalculatorMock.Object, imm, res, vuln, flags);

            bool savesSpell = victim.SavesSpell(10, damType);

            if (expected.HasValue)
                randomManagerMock.Verify(x => x.Chance(expected.Value), Times.Once);
            else
                randomManagerMock.Verify(x => x.Chance(It.IsAny<int>()), Times.Never); // no call to RandomManager.Chance because immune to Acid
        }

        private ResistanceLevels CheckResistances(ICharacter victim, SchoolTypes schoolTypes)
        {
            var irvFlag = schoolTypes.ToString();
            if (victim.Immunities.IsSet(irvFlag))
                return ResistanceLevels.Immune;
            if (victim.Resistances.IsSet(irvFlag))
                return ResistanceLevels.Resistant;
            if (victim.Vulnerabilities.IsSet(irvFlag))
                return ResistanceLevels.Vulnerable;
            return ResistanceLevels.Normal;
        }

        private static INonPlayableCharacter GenerateNPC(IRandomManager randomManager, IResistanceCalculator resistanceCalculator, string imm, string res, string vuln, string characterFlags)
        {
            var loggerMock = new Mock<ILogger<NonPlayableCharacter>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var classManagerMock = new Mock<IClassManager>();
            var raceManagerMock = new Mock<IRaceManager>();
            var roomMock = new Mock<IRoom>();

            classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
            raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IRace>().Object);

            var blueprint = new CharacterNormalBlueprint
            {
                // CharacterBaseData
                Name = "NPC",
                Race = "Human",
                Class = "Warrior",
                Level = 6,
                Sex = Sex.Male,
                Size = Sizes.Medium,
                CharacterFlags = new CharacterFlags(characterFlags),
                Immunities = new IRVFlags(imm),
                Resistances = new IRVFlags(res),
                Vulnerabilities = new IRVFlags(vuln),
                ActFlags = new ActFlags(),
                OffensiveFlags = new OffensiveFlags(),
                ShieldFlags = new ShieldFlags(),
            };

            var npc = new NonPlayableCharacter(loggerMock.Object, null!, null!, null!, messageForwardOptions, randomManager, null!, null!, null!, null!, null!, null!, null!, raceManagerMock.Object, classManagerMock.Object, resistanceCalculator, null!, null!, null!);
            npc.Initialize(Guid.NewGuid(), blueprint, roomMock.Object);

            return npc;
        }
    }
}
