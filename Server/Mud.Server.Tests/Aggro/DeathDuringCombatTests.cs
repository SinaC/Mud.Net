using Microsoft.Extensions.Logging;
using Moq;
using Mud.Random;
using Mud.Server.Combat;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Tests.Aggro;

[TestClass]
public class DeathDuringCombatTests : CombatTestsBase
{
    [TestMethod]
    public void PcKillsNpc()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        var pc1 = GeneratePC("pc", aggroManager, roomMock.Object);
        var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc1, npc]);

        aggroManager.OnStartFight(pc1, npc);
        aggroManager.OnReceiveDamage(pc1, npc, 1000);
        aggroManager.OnDeath(npc);

        Assert.IsNull(aggroManager.GetAggroTable(npc));
    }

    [TestMethod]
    public void NpcKillsPc()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        var pc1 = GeneratePC("pc", aggroManager, roomMock.Object);
        var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc1, npc]);

        aggroManager.OnStartFight(pc1, npc);
        aggroManager.OnReceiveDamage(npc, pc1, 1000);
        aggroManager.OnDeath(pc1);

        Assert.IsEmpty(aggroManager.GetAggroTable(npc)!.AggroByEnemy.Keys);
    }

    [TestMethod]
    public void PcPet_PcKillsNpc()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        var pc1 = GeneratePC("pc", aggroManager, roomMock.Object);
        var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
        var pet = GenerateNPC("pet", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc1, npc, pet]);
        pet.ActFlags.Set("pet");
        pet.CharacterFlags.Set("charm");
        pc1.AddPet(pet);

        aggroManager.OnStartFight(pc1, npc);
        aggroManager.OnStartFight(pet, npc);
        aggroManager.OnReceiveDamage(npc, pc1, 1000);
        aggroManager.OnReceiveDamage(pet, npc, 1);
        aggroManager.OnDeath(npc);

        Assert.IsNull(aggroManager.GetAggroTable(npc));
        Assert.IsNull(aggroManager.GetAggroTable(pet)); // no aggro table for pet
    }

    [TestMethod]
    public void Group_LeaderKillsNpc()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        var pc1 = GeneratePC("pc1", aggroManager, roomMock.Object);
        var pc2 = GeneratePC("pc2", aggroManager, roomMock.Object);
        var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc1, pc2, npc]);
        // create a group with pc1 as leader and pc2 as member
        var group = new Group.Group(pc1);
        pc2.ChangeGroup(group);

        aggroManager.OnStartFight(pc1, npc);
        aggroManager.OnStartFight(pc2, npc);
        aggroManager.OnReceiveDamage(pc1, npc, 1000);
        aggroManager.OnDeath(npc);

        Assert.IsNull(aggroManager.GetAggroTable(npc));
    }

    [TestMethod]
    public void Group_NpcKillsLeader()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        var pc1 = GeneratePC("pc1", aggroManager, roomMock.Object);
        var pc2 = GeneratePC("pc2", aggroManager, roomMock.Object);
        var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc1, pc2, npc]);
        // create a group with pc1 as leader and pc2 as member
        var group = new Group.Group(pc1);
        pc2.ChangeGroup(group);

        aggroManager.OnStartFight(pc1, npc);
        aggroManager.OnStartFight(pc2, npc);
        aggroManager.OnReceiveDamage(pc1, npc, 1000);
        aggroManager.OnDeath(pc1);

        Assert.IsNotNull(aggroManager.GetAggroTable(npc));
        Assert.IsNotEmpty(aggroManager.GetAggroTable(npc)!.AggroByEnemy);
        Assert.AreEqual(pc2, aggroManager.GetAggroTable(npc)!.GetEnemyInRoom(npc.Room!));
    }
}
