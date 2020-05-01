using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Mud.Server.Tests
{
    // TODO: CharacterFlagsAffect, CharacterIRVAffect, CharacterSexAffect
    [TestClass]
    public class CharacterAffectTests
    {
        [TestMethod]
        public void OneNormalAttribute_Add_Test()
        {
            INonPlayableCharacter npc = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1" }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("Area1", 1, 100, "builders", "credits")));
            Aura.CharacterAttributeAffect strAffect = new Aura.CharacterAttributeAffect
            {
                Location = Domain.CharacterAttributeAffectLocations.Strength,
                Operator = Domain.AffectOperators.Add,
                Modifier = 6
            };

            int originalBaseStr = npc.BaseAttributes(Domain.CharacterAttributes.Strength);
            int originalCurrentStr = npc.CurrentAttributes(Domain.CharacterAttributes.Strength);
            npc.ApplyAffect(strAffect);

            Assert.AreEqual(originalBaseStr, npc.BaseAttributes(Domain.CharacterAttributes.Strength));
            Assert.AreEqual(originalCurrentStr + 6, npc.CurrentAttributes(Domain.CharacterAttributes.Strength));
        }

        [TestMethod]
        public void OneNormalAttribute_Assign_Test()
        {
            INonPlayableCharacter npc = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1" }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("Area1", 1, 100, "builders", "credits")));
            Aura.CharacterAttributeAffect intAffect = new Aura.CharacterAttributeAffect
            {
                Location = Domain.CharacterAttributeAffectLocations.Intelligence,
                Operator = Domain.AffectOperators.Assign,
                Modifier = 6
            };

            int originalBaseInt = npc.BaseAttributes(Domain.CharacterAttributes.Intelligence);
            npc.ApplyAffect(intAffect);

            Assert.AreEqual(originalBaseInt, npc.BaseAttributes(Domain.CharacterAttributes.Intelligence));
            Assert.AreEqual(6, npc.CurrentAttributes(Domain.CharacterAttributes.Intelligence));
        }

        [TestMethod]
        public void OneCombinedAttribute_Add_Test()
        {
            INonPlayableCharacter npc = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1" }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("Area1", 1, 100, "builders", "credits")));
            Aura.CharacterAttributeAffect caracAffect = new Aura.CharacterAttributeAffect
            {
                Location = Domain.CharacterAttributeAffectLocations.Characteristics,
                Operator = Domain.AffectOperators.Add,
                Modifier = 3
            };

            int originalBaseStr = npc.BaseAttributes(Domain.CharacterAttributes.Strength);
            int originalCurrentStr = npc.CurrentAttributes(Domain.CharacterAttributes.Strength);
            int originalBaseInt = npc.BaseAttributes(Domain.CharacterAttributes.Intelligence);
            int originalCurrentInt = npc.CurrentAttributes(Domain.CharacterAttributes.Intelligence);
            int originalBaseWis = npc.BaseAttributes(Domain.CharacterAttributes.Wisdom);
            int originalCurrentWis = npc.CurrentAttributes(Domain.CharacterAttributes.Wisdom);
            int originalBaseDex = npc.BaseAttributes(Domain.CharacterAttributes.Dexterity);
            int originalCurrentDex = npc.CurrentAttributes(Domain.CharacterAttributes.Dexterity);
            int originalBaseCon = npc.BaseAttributes(Domain.CharacterAttributes.Constitution);
            int originalCurrentCon = npc.CurrentAttributes(Domain.CharacterAttributes.Constitution);
            npc.ApplyAffect(caracAffect);

            Assert.AreEqual(originalBaseStr, npc.BaseAttributes(Domain.CharacterAttributes.Strength));
            Assert.AreEqual(originalCurrentStr + 3, npc.CurrentAttributes(Domain.CharacterAttributes.Strength));
            Assert.AreEqual(originalBaseInt, npc.BaseAttributes(Domain.CharacterAttributes.Intelligence));
            Assert.AreEqual(originalCurrentInt + 3, npc.CurrentAttributes(Domain.CharacterAttributes.Intelligence));
            Assert.AreEqual(originalBaseWis, npc.BaseAttributes(Domain.CharacterAttributes.Wisdom));
            Assert.AreEqual(originalCurrentWis + 3, npc.CurrentAttributes(Domain.CharacterAttributes.Wisdom));
            Assert.AreEqual(originalBaseDex, npc.BaseAttributes(Domain.CharacterAttributes.Dexterity));
            Assert.AreEqual(originalCurrentDex + 3, npc.CurrentAttributes(Domain.CharacterAttributes.Dexterity));
            Assert.AreEqual(originalBaseCon, npc.BaseAttributes(Domain.CharacterAttributes.Constitution));
            Assert.AreEqual(originalCurrentCon + 3, npc.CurrentAttributes(Domain.CharacterAttributes.Constitution));
        }

        [TestMethod]
        public void OneCombinedAttribute_Assign_Test()
        {
            INonPlayableCharacter npc = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1" }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("Area1", 1, 100, "builders", "credits")));
            Aura.CharacterAttributeAffect caracAffect = new Aura.CharacterAttributeAffect
            {
                Location = Domain.CharacterAttributeAffectLocations.AllArmor,
                Operator = Domain.AffectOperators.Assign,
                Modifier = 3
            };

            int originalBaseBash = npc.BaseAttributes(Domain.CharacterAttributes.ArmorBash);
            int originalBasePierce = npc.BaseAttributes(Domain.CharacterAttributes.ArmorPierce);
            int originalBaseSlash = npc.BaseAttributes(Domain.CharacterAttributes.ArmorSlash);
            int originalBaseMagic = npc.BaseAttributes(Domain.CharacterAttributes.ArmorMagic);
            npc.ApplyAffect(caracAffect);

            Assert.AreEqual(originalBaseBash, npc.BaseAttributes(Domain.CharacterAttributes.ArmorBash));
            Assert.AreEqual(3, npc.CurrentAttributes(Domain.CharacterAttributes.ArmorBash));
            Assert.AreEqual(originalBasePierce, npc.BaseAttributes(Domain.CharacterAttributes.ArmorPierce));
            Assert.AreEqual(3, npc.CurrentAttributes(Domain.CharacterAttributes.ArmorPierce));
            Assert.AreEqual(originalBaseSlash, npc.BaseAttributes(Domain.CharacterAttributes.ArmorSlash));
            Assert.AreEqual(3, npc.CurrentAttributes(Domain.CharacterAttributes.ArmorSlash));
            Assert.AreEqual(originalBaseMagic, npc.BaseAttributes(Domain.CharacterAttributes.ArmorMagic));
            Assert.AreEqual(3, npc.CurrentAttributes(Domain.CharacterAttributes.ArmorMagic));
        }
    }
}
