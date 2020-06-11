using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using Mud.Server.Interfaces.Character;
using Mud.Server.Affect;
using Mud.Server.Interfaces.Area;

namespace Mud.Server.Tests.Affects
{
    // TODO: CharacterFlagsAffect, CharacterIRVAffect, CharacterSexAffect
    [TestClass]
    public class CharacterAffectTests
    {
        [TestMethod]
        public void OneNormalAttribute_Add_Test()
        {
            INonPlayableCharacter npc = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1" }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));
            CharacterAttributeAffect strAffect = new CharacterAttributeAffect
            {
                Location = Domain.CharacterAttributeAffectLocations.Strength,
                Operator = Domain.AffectOperators.Add,
                Modifier = 6
            };

            int originalBaseStr = npc.BaseAttribute(Domain.CharacterAttributes.Strength);
            int originalCurrentStr = npc[Domain.CharacterAttributes.Strength];
            npc.ApplyAffect(strAffect);

            Assert.AreEqual(originalBaseStr, npc.BaseAttribute(Domain.CharacterAttributes.Strength));
            Assert.AreEqual(originalCurrentStr + 6, npc[Domain.CharacterAttributes.Strength]);
        }

        [TestMethod]
        public void OneNormalAttribute_Assign_Test()
        {
            INonPlayableCharacter npc = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1" }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));
            CharacterAttributeAffect intAffect = new CharacterAttributeAffect
            {
                Location = Domain.CharacterAttributeAffectLocations.Intelligence,
                Operator = Domain.AffectOperators.Assign,
                Modifier = 6
            };

            int originalBaseInt = npc.BaseAttribute(Domain.CharacterAttributes.Intelligence);
            npc.ApplyAffect(intAffect);

            Assert.AreEqual(originalBaseInt, npc.BaseAttribute(Domain.CharacterAttributes.Intelligence));
            Assert.AreEqual(6, npc[Domain.CharacterAttributes.Intelligence]);
        }

        [TestMethod]
        public void OneCombinedAttribute_Add_Test()
        {
            INonPlayableCharacter npc = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1" }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));
            CharacterAttributeAffect caracAffect = new CharacterAttributeAffect
            {
                Location = Domain.CharacterAttributeAffectLocations.Characteristics,
                Operator = Domain.AffectOperators.Add,
                Modifier = 3
            };

            int originalBaseStr = npc.BaseAttribute(Domain.CharacterAttributes.Strength);
            int originalCurrentStr = npc[Domain.CharacterAttributes.Strength];
            int originalBaseInt = npc.BaseAttribute(Domain.CharacterAttributes.Intelligence);
            int originalCurrentInt = npc[Domain.CharacterAttributes.Intelligence];
            int originalBaseWis = npc.BaseAttribute(Domain.CharacterAttributes.Wisdom);
            int originalCurrentWis = npc[Domain.CharacterAttributes.Wisdom];
            int originalBaseDex = npc.BaseAttribute(Domain.CharacterAttributes.Dexterity);
            int originalCurrentDex = npc[Domain.CharacterAttributes.Dexterity];
            int originalBaseCon = npc.BaseAttribute(Domain.CharacterAttributes.Constitution);
            int originalCurrentCon = npc[Domain.CharacterAttributes.Constitution];
            npc.ApplyAffect(caracAffect);

            Assert.AreEqual(originalBaseStr, npc.BaseAttribute(Domain.CharacterAttributes.Strength));
            Assert.AreEqual(originalCurrentStr + 3, npc[Domain.CharacterAttributes.Strength]);
            Assert.AreEqual(originalBaseInt, npc.BaseAttribute(Domain.CharacterAttributes.Intelligence));
            Assert.AreEqual(originalCurrentInt + 3, npc[Domain.CharacterAttributes.Intelligence]);
            Assert.AreEqual(originalBaseWis, npc.BaseAttribute(Domain.CharacterAttributes.Wisdom));
            Assert.AreEqual(originalCurrentWis + 3, npc[Domain.CharacterAttributes.Wisdom]);
            Assert.AreEqual(originalBaseDex, npc.BaseAttribute(Domain.CharacterAttributes.Dexterity));
            Assert.AreEqual(originalCurrentDex + 3, npc[Domain.CharacterAttributes.Dexterity]);
            Assert.AreEqual(originalBaseCon, npc.BaseAttribute(Domain.CharacterAttributes.Constitution));
            Assert.AreEqual(originalCurrentCon + 3, npc[Domain.CharacterAttributes.Constitution]);
        }

        [TestMethod]
        public void OneCombinedAttribute_Assign_Test()
        {
            INonPlayableCharacter npc = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1" }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));
            CharacterAttributeAffect caracAffect = new CharacterAttributeAffect
            {
                Location = Domain.CharacterAttributeAffectLocations.AllArmor,
                Operator = Domain.AffectOperators.Assign,
                Modifier = 3
            };

            int originalBaseBash = npc.BaseAttribute(Domain.CharacterAttributes.ArmorBash);
            int originalBasePierce = npc.BaseAttribute(Domain.CharacterAttributes.ArmorPierce);
            int originalBaseSlash = npc.BaseAttribute(Domain.CharacterAttributes.ArmorSlash);
            int originalBaseMagic = npc.BaseAttribute(Domain.CharacterAttributes.ArmorExotic);
            npc.ApplyAffect(caracAffect);

            Assert.AreEqual(originalBaseBash, npc.BaseAttribute(Domain.CharacterAttributes.ArmorBash));
            Assert.AreEqual(3, npc[Domain.CharacterAttributes.ArmorBash]);
            Assert.AreEqual(originalBasePierce, npc.BaseAttribute(Domain.CharacterAttributes.ArmorPierce));
            Assert.AreEqual(3, npc[Domain.CharacterAttributes.ArmorPierce]);
            Assert.AreEqual(originalBaseSlash, npc.BaseAttribute(Domain.CharacterAttributes.ArmorSlash));
            Assert.AreEqual(3, npc[Domain.CharacterAttributes.ArmorSlash]);
            Assert.AreEqual(originalBaseMagic, npc.BaseAttribute(Domain.CharacterAttributes.ArmorExotic));
            Assert.AreEqual(3, npc[Domain.CharacterAttributes.ArmorExotic]);
        }
    }
}
