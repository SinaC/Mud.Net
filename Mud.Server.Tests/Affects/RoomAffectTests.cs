using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Domain;
using Mud.Server.Item;
using System;
using Moq;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Item;
using Mud.Server.Affects;
using Mud.Server.Flags;

namespace Mud.Server.Tests.Affects
{
    /*
    [TestClass]
    public class RoomAffectTests
    {
        // Room priority affects: room, then characters, then items

        // One room affect on room
        [TestMethod]
        public void Room_OneRoomAddAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags() }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark")});
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsNone);
            Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
        }

        [TestMethod]
        public void Room_OneRoomAddAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags("Dark") }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsSet("Dark"));
            Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
        }

        [TestMethod]
        public void Room_OneRoomAddAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags("ImpOnly") }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsSet("ImpOnly"));
            Assert.IsTrue(room.RoomFlags.HasAll("Dark", "ImpOnly"));
        }

        [TestMethod]
        public void Room_OneRoomOrAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags() }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsNone);
            Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
        }

        [TestMethod]
        public void Room_OneRoomOrAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags("Dark") }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsSet("Dark"));
            Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
        }

        [TestMethod]
        public void Room_OneRoomOrAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags("ImpOnly") }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsSet("ImpOnly"));
            Assert.IsTrue(room.RoomFlags.HasAll("Dark", "ImpOnly"));
        }

        [TestMethod]
        public void Room_OneRoomAssignAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags() }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsNone);
            Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
        }

        [TestMethod]
        public void Room_OneRoomAssignAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags("Dark") }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsSet("Dark"));
            Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
        }

        [TestMethod]
        public void Room_OneRoomAssignAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags("ImpOnly") }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsSet("ImpOnly"));
            Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
        }

        [TestMethod]
        public void Room_OneRoomNorAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags() }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsNone);
            Assert.IsTrue(room.RoomFlags.IsNone);
        }

        [TestMethod]
        public void Room_OneRoomNorAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags("Dark") }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsSet("Dark"));
            Assert.IsTrue(room.RoomFlags.IsNone);
        }

        [TestMethod]
        public void Room_OneRoomNorAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags("ImpOnly") }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsSet("ImpOnly"));
            Assert.IsTrue(room.RoomFlags.IsSet("ImpOnly"));
        }

        // TODO
        // One room affect on item in room
        // one test for each combination (4*4*2)
        // room ADD/ASSIGN/OR/NOR item ADD/ASSIGN/OR/NOR  on same and different value
        // a==a room  + = | ~       a<>b room(a) +  =  |  ~
        // item                     item(b)
        // +          a a a a       +           ab ab ab  b
        // =          a a a a       =            b  b  b  b
        // |          a a a a       |           ab ab  ab b
        // ~          / / / /       ~            a  a  a  /
        [TestMethod]
        public void Room_OneRoomAddAffect_OneItemAddAffect_Identical_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = new RoomFlags() }, new Mock<IArea>().Object);
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark") });
            room.AddAura(roomAura, false);
            IItem item = new ItemArmor(Guid.NewGuid(), new Blueprints.Item.ItemArmorBlueprint { Id = 1, Name = "item1", ShortDescription = "item1short", Description = "item1desc" }, room);
            IAura itemAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark") });
            item.AddAura(itemAura, false);

            room.Recompute();

            Assert.IsTrue(room.BaseRoomFlags.IsNone);
            Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
        }

        // TODO
        // One room affect on character in room

        // TODO
        // One room affect on room + one room affect on item in room

        // TODO
        // One room affect on room + one room affect on character in room

        // TODO
        // One room affect on room + one room affect on item in room + one room affect on character in room
    }
    */
}
