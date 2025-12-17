using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Tests.NonPlayableCharacters
{
    [TestClass]
    public class CtorAndInitializeTests : TestBase
    {
        [TestMethod]
        public void Attributes()
        {
            var room = new Mock<IRoom>();

            var blueprint = GenerateBluePrint();
            var npc = GenerateNPC(blueprint, room.Object);
            npc.Recompute();

            Assert.AreEqual(npc[CharacterAttributes.Strength], npc[BasicAttributes.Strength]);
            Assert.AreEqual(npc[CharacterAttributes.Intelligence], npc[BasicAttributes.Intelligence]);
            Assert.AreEqual(npc[CharacterAttributes.Wisdom], npc[BasicAttributes.Wisdom]);
            Assert.AreEqual(npc[CharacterAttributes.Dexterity], npc[BasicAttributes.Dexterity]);
            Assert.AreEqual(npc[CharacterAttributes.Constitution], npc[BasicAttributes.Constitution]);
        }

        [TestMethod]
        public void HitPoints()
        {
            var room = new Mock<IRoom>();
            var blueprint = GenerateBluePrint();

            var npc = GenerateNPC(blueprint, room.Object);
            npc.Recompute();

            Assert.AreEqual(blueprint.HitPointDiceCount * blueprint.HitPointDiceValue + blueprint.HitPointDiceBonus, npc.MaxHitPoints);
            Assert.AreEqual(blueprint.HitPointDiceCount * blueprint.HitPointDiceValue + blueprint.HitPointDiceBonus, npc[CharacterAttributes.MaxHitPoints]);
            Assert.AreEqual(blueprint.HitPointDiceCount * blueprint.HitPointDiceValue + blueprint.HitPointDiceBonus, npc.CurrentHitPoints);
        }

        [TestMethod]
        public void Mana()
        {
            var room = new Mock<IRoom>();
            var blueprint = GenerateBluePrint();

            var npc = GenerateNPC(blueprint, room.Object);
            npc.Recompute();

            Assert.AreEqual(blueprint.ManaDiceCount * blueprint.ManaDiceValue + blueprint.ManaDiceBonus, npc.MaxResource(ResourceKinds.Mana));
            Assert.AreEqual(blueprint.ManaDiceCount * blueprint.ManaDiceValue + blueprint.ManaDiceBonus, npc[ResourceKinds.Mana]);
        }

        [TestMethod]
        public void Armor()
        {
            int defensiveBonus = 50;
            var room = new Mock<IRoom>();
            var blueprint = GenerateBluePrint();

            var npc = GenerateNPC(blueprint, room.Object);
            npc.Recompute();

            Assert.AreEqual(blueprint.ArmorBash + defensiveBonus, npc[Armors.Bash]); // TableValues armor has been mocked to level (50)
            Assert.AreEqual(blueprint.ArmorPierce + defensiveBonus, npc[Armors.Pierce]);
            Assert.AreEqual(blueprint.ArmorSlash + defensiveBonus, npc[Armors.Slash]);
            Assert.AreEqual(blueprint.ArmorExotic + defensiveBonus, npc[Armors.Exotic]);
        }

        [TestMethod]
        public void HitDamRoll()
        {
            int hitBonus = 45;
            int damBonus = 30;
            var room = new Mock<IRoom>();
            var blueprint = GenerateBluePrint();

            var npc = GenerateNPC(blueprint, room.Object);
            npc.Recompute();

            Assert.AreEqual(blueprint.HitRollBonus + hitBonus, npc.HitRoll); // TableValues hitroll has been mocked to level-5 (50)
            Assert.AreEqual(npc.Level + damBonus, npc.DamRoll); // TableValues damroll has been mocked to level-20 (50)
        }

        private static CharacterNormalBlueprint GenerateBluePrint()
            => new ()
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
            };
    }
}
