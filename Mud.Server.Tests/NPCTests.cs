﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Room;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using System;
using System.Linq;
using Moq;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Tests
{
    [TestClass]
    public class NPCTests
    {
        //

        // IRV tests
        [TestMethod]
        public void CheckResistance_DamageTypeNone_NoIrv_Test() 
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1, Immunities = new IRVFlags() }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));

            var level = victim.CheckResistance(SchoolTypes.None);

            Assert.AreEqual(ResistanceLevels.None, level);
        }

        [TestMethod]
        public void CheckResistance_DamageTypeNone_ImmuneAll_Test()
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1, Immunities = IRVAll }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));

            var level = victim.CheckResistance(SchoolTypes.None);

            Assert.AreEqual(ResistanceLevels.None, level);
        }

        // TODO: more tests

        private IIRVFlags IRVAll => new IRVFlags(Rom24IRVFlagValues.Flags.ToArray());
    }
}
