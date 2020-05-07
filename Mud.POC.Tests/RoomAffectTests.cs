using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.POC.Affects;

namespace Mud.POC.Tests
{
    [TestClass]
    public class RoomAffectTests
    {
        // Room priority affects: room, then characters, then items

        // One room affect on room
        [TestMethod]
        public void Room_OneRoomAddAffect_NoBaseValue_Test()
        {
            IRoom room = new Room("room1");
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.None, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAddAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room("room1", RoomFlags.Dark);
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.Dark, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAddAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room("room1", RoomFlags.ImpOnly);
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.ImpOnly, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark | RoomFlags.ImpOnly, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomOrAffect_NoBaseValue_Test()
        {
            IRoom room = new Room("room1");
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.None, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomOrAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room("room1", RoomFlags.Dark);
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.Dark, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomOrAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room("room1", RoomFlags.ImpOnly);
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.ImpOnly, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark | RoomFlags.ImpOnly, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAssignAffect_NoBaseValue_Test()
        {
            IRoom room = new Room("room1");
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.None, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAssignAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room("room1", RoomFlags.Dark);
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.Dark, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomAssignAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room("room1", RoomFlags.ImpOnly);
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.ImpOnly, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.Dark, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomNorAffect_NoBaseValue_Test()
        {
            IRoom room = new Room("room1");
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.None, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.None, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomNorAffect_IdenticalBaseValue_Test()
        {
            IRoom room = new Room("room1", RoomFlags.Dark);
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            room.Recompute();

            Assert.AreEqual(RoomFlags.Dark, room.BaseRoomFlags);
            Assert.AreEqual(RoomFlags.None, room.CurrentRoomFlags);
        }

        [TestMethod]
        public void Room_OneRoomNorAffect_DifferentBaseValue_Test()
        {
            IRoom room = new Room("room1", RoomFlags.ImpOnly);
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
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
            IRoom room = new Room("room1", RoomFlags.None);
            IAura roomAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            room.AddAura(roomAura);
            IItem item = new ItemDummy("item1", room, null);
            IAura itemAura = new Aura(null, null, AuraFlags.Permanent, 10, 10, new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = RoomFlags.Dark });
            item.AddAura(itemAura);
            room.AddItem(item);

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
