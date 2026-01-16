using Mud.Domain;
using Mud.Server.Affects.Room;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Server.Interfaces.Aura;

namespace Mud.Server.Tests.Affects;

[TestClass]
public class RoomAffectTests : TestBase
{
    // Room priority affects: room, then characters, then items

    // One room affect on room
    [TestMethod]
    public void OneRoomAddAffect_NoBaseValue()
    {
        var room = GenerateRoom("");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark")});
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsNone);
        Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
    }

    [TestMethod]
    public void OneRoomAddAffect_IdenticalBaseValue()
    {
        var room = GenerateRoom("Dark");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsSet("Dark"));
        Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
    }

    [TestMethod]
    public void OneRoomAddAffect_DifferentBaseValue()
    {
        var room = GenerateRoom("ImpOnly");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsSet("ImpOnly"));
        Assert.IsTrue(room.RoomFlags.HasAll("Dark", "ImpOnly"));
    }

    [TestMethod]
    public void OneRoomOrAffect_NoBaseValue()
    {
        var room = GenerateRoom("");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsNone);
        Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
    }

    [TestMethod]
    public void OneRoomOrAffect_IdenticalBaseValue()
    {
        var room = GenerateRoom("Dark");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsSet("Dark"));
        Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
    }

    [TestMethod]
    public void OneRoomOrAffect_DifferentBaseValue()
    {
        var room = GenerateRoom("ImpOnly");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Or, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsSet("ImpOnly"));
        Assert.IsTrue(room.RoomFlags.HasAll("Dark", "ImpOnly"));
    }

    [TestMethod]
    public void OneRoomAssignAffect_NoBaseValue()
    {
        var room = GenerateRoom("");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsNone);
        Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
    }

    [TestMethod]
    public void OneRoomAssignAffect_IdenticalBaseValue()
    {
        var room = GenerateRoom("Dark");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsSet("Dark"));
        Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
    }

    [TestMethod]
    public void OneRoomAssignAffect_DifferentBaseValue()
    {
        var room = GenerateRoom("ImpOnly");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Assign, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsSet("ImpOnly"));
        Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
    }

    [TestMethod]
    public void OneRoomNorAffect_NoBaseValue()
    {
        var room = GenerateRoom("");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsNone);
        Assert.IsTrue(room.RoomFlags.IsNone);
    }

    [TestMethod]
    public void OneRoomNorAffect_IdenticalBaseValue()
    {
        var room = GenerateRoom("Dark");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsSet("Dark"));
        Assert.IsTrue(room.RoomFlags.IsNone);
    }

    [TestMethod]
    public void OneRoomNorAffect_DifferentBaseValue()
    {
        var room = GenerateRoom("ImpOnly");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Nor, Modifier = new RoomFlags("Dark") });
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
    public void OneRoomAddAffect_OneItemAddAffect_Identical()
    {
        var room = GenerateRoom("");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark") });
        room.AddAura(roomAura, false);
        var item = GenerateArmor("", room);
        IAura itemAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark") });
        item.AddAura(itemAura, false);

        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsNone);
        Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
    }

    [TestMethod]
    public void OneRoomAddAffect_OneItemAddAffect_Different()
    {
        var room = GenerateRoom("");
        var roomAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("ImpOnly") });
        room.AddAura(roomAura, false);
        var item = GenerateArmor("", room);
        IAura itemAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new RoomFlagsAffect { Operator = AffectOperators.Add, Modifier = new RoomFlags("Dark") });
        item.AddAura(itemAura, false);

        room.Recompute();

        Assert.IsTrue(room.BaseRoomFlags.IsNone);
        Assert.IsTrue(room.RoomFlags.IsSet("Dark"));
        Assert.IsTrue(room.RoomFlags.IsSet("ImpOnly"));
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
