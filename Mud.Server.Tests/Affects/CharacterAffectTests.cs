﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Mud.Server.Tests.Affects
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

            int originalBaseStr = npc.BaseAttribute(Domain.CharacterAttributes.Strength);
            int originalCurrentStr = npc.CurrentAttribute(Domain.CharacterAttributes.Strength);
            npc.ApplyAffect(strAffect);

            Assert.AreEqual(originalBaseStr, npc.BaseAttribute(Domain.CharacterAttributes.Strength));
            Assert.AreEqual(originalCurrentStr + 6, npc.CurrentAttribute(Domain.CharacterAttributes.Strength));
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

            int originalBaseInt = npc.BaseAttribute(Domain.CharacterAttributes.Intelligence);
            npc.ApplyAffect(intAffect);

            Assert.AreEqual(originalBaseInt, npc.BaseAttribute(Domain.CharacterAttributes.Intelligence));
            Assert.AreEqual(6, npc.CurrentAttribute(Domain.CharacterAttributes.Intelligence));
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

            int originalBaseStr = npc.BaseAttribute(Domain.CharacterAttributes.Strength);
            int originalCurrentStr = npc.CurrentAttribute(Domain.CharacterAttributes.Strength);
            int originalBaseInt = npc.BaseAttribute(Domain.CharacterAttributes.Intelligence);
            int originalCurrentInt = npc.CurrentAttribute(Domain.CharacterAttributes.Intelligence);
            int originalBaseWis = npc.BaseAttribute(Domain.CharacterAttributes.Wisdom);
            int originalCurrentWis = npc.CurrentAttribute(Domain.CharacterAttributes.Wisdom);
            int originalBaseDex = npc.BaseAttribute(Domain.CharacterAttributes.Dexterity);
            int originalCurrentDex = npc.CurrentAttribute(Domain.CharacterAttributes.Dexterity);
            int originalBaseCon = npc.BaseAttribute(Domain.CharacterAttributes.Constitution);
            int originalCurrentCon = npc.CurrentAttribute(Domain.CharacterAttributes.Constitution);
            npc.ApplyAffect(caracAffect);

            Assert.AreEqual(originalBaseStr, npc.BaseAttribute(Domain.CharacterAttributes.Strength));
            Assert.AreEqual(originalCurrentStr + 3, npc.CurrentAttribute(Domain.CharacterAttributes.Strength));
            Assert.AreEqual(originalBaseInt, npc.BaseAttribute(Domain.CharacterAttributes.Intelligence));
            Assert.AreEqual(originalCurrentInt + 3, npc.CurrentAttribute(Domain.CharacterAttributes.Intelligence));
            Assert.AreEqual(originalBaseWis, npc.BaseAttribute(Domain.CharacterAttributes.Wisdom));
            Assert.AreEqual(originalCurrentWis + 3, npc.CurrentAttribute(Domain.CharacterAttributes.Wisdom));
            Assert.AreEqual(originalBaseDex, npc.BaseAttribute(Domain.CharacterAttributes.Dexterity));
            Assert.AreEqual(originalCurrentDex + 3, npc.CurrentAttribute(Domain.CharacterAttributes.Dexterity));
            Assert.AreEqual(originalBaseCon, npc.BaseAttribute(Domain.CharacterAttributes.Constitution));
            Assert.AreEqual(originalCurrentCon + 3, npc.CurrentAttribute(Domain.CharacterAttributes.Constitution));
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

            int originalBaseBash = npc.BaseAttribute(Domain.CharacterAttributes.ArmorBash);
            int originalBasePierce = npc.BaseAttribute(Domain.CharacterAttributes.ArmorPierce);
            int originalBaseSlash = npc.BaseAttribute(Domain.CharacterAttributes.ArmorSlash);
            int originalBaseMagic = npc.BaseAttribute(Domain.CharacterAttributes.ArmorMagic);
            npc.ApplyAffect(caracAffect);

            Assert.AreEqual(originalBaseBash, npc.BaseAttribute(Domain.CharacterAttributes.ArmorBash));
            Assert.AreEqual(3, npc.CurrentAttribute(Domain.CharacterAttributes.ArmorBash));
            Assert.AreEqual(originalBasePierce, npc.BaseAttribute(Domain.CharacterAttributes.ArmorPierce));
            Assert.AreEqual(3, npc.CurrentAttribute(Domain.CharacterAttributes.ArmorPierce));
            Assert.AreEqual(originalBaseSlash, npc.BaseAttribute(Domain.CharacterAttributes.ArmorSlash));
            Assert.AreEqual(3, npc.CurrentAttribute(Domain.CharacterAttributes.ArmorSlash));
            Assert.AreEqual(originalBaseMagic, npc.BaseAttribute(Domain.CharacterAttributes.ArmorMagic));
            Assert.AreEqual(3, npc.CurrentAttribute(Domain.CharacterAttributes.ArmorMagic));
        }
    }
}
