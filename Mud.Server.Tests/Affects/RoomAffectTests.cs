using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Domain;
using Mud.Server.Aura;
using Mud.Server.Item;
using System;

namespace Mud.Server.Tests.Affects
{
    [TestClass]
    public class RoomAffectTests
    {
        // Room priority affects: room, then characters, then items

        // One room affect on room
        [TestMethod]
        public void Room_OneRoomAddAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.None, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAddAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = RoomFlags.Dark }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.Dark, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAddAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = RoomFlags.ImpOnly }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.ImpOnly, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark | RoomFlags.ImpOnly, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomOrAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.None, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomOrAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = RoomFlags.Dark }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.Dark, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomOrAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = RoomFlags.ImpOnly }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.ImpOnly, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark | RoomFlags.ImpOnly, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAssignAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.None, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAssignAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = RoomFlags.Dark }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.Dark, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAssignAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = RoomFlags.ImpOnly }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.ImpOnly, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomNorAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.None, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.None, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomNorAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = RoomFlags.Dark }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.Dark, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.None, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomNorAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1", RoomFlags = RoomFlags.ImpOnly }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            room.Recompute();

            Assert.AreEqual(RoomFlags.ImpOnly, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.ImpOnly, room.CurrentRoomFlags);
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
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IAura roomAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura, false);
            IItem item = new ItemArmor(Guid.NewGuid(), new Blueprints.Item.ItemArmorBlueprint { Id = 1, Name = "item1", ShortDescription = "item1short", Description = "item1desc" }, room);
            IAura itemAura = new Aura.Aura(null, null, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            item.AddAura(itemAura, false);

            room.Recompute();

            Assert.AreEqual(RoomFlags.None, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
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
}
