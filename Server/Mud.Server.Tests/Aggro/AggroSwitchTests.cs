using Microsoft.Extensions.Logging;
using Moq;
using Mud.Random;
using Mud.Server.Combat;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Tests.Aggro;

[TestClass]
public class AggroSwitchTests : CombatTestsBase
{
    [TestMethod]
    public void TwoRounds_Group_AggroSwitch()
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
        //
        aggroManager.OnReceiveDamage(pc1, npc, 4 * 10);
        aggroManager.OnReceiveDamage(pc2, npc, 1 * 10);
        //
        aggroManager.OnReceiveDamage(pc1, npc, 4 * 10);
        aggroManager.OnReceiveDamage(pc2, npc, 1 * 100);
        // pc1 generated 1+4*10+4*10 = 81 aggro
        // pc2 generated 1+1*10+1*100 = 111 aggro

        Assert.AreEqual(pc2, aggroManager.GetAggroTable(npc)!.GetEnemyInRoom(npc.Room!));
        Assert.AreEqual(81, aggroManager.GetAggroTable(npc)!.AggroByEnemy[pc1]);
        Assert.AreEqual(111, aggroManager.GetAggroTable(npc)!.AggroByEnemy[pc2]);
    }
}
