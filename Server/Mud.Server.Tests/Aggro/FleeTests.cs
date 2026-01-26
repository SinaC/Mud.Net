using Microsoft.Extensions.Logging;
using Moq;
using Mud.Random;
using Mud.Server.Combat;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Tests.Aggro
{
    [TestClass]
    public class FleeTests : CombatTestsBase
    {
        [TestMethod]
        public void Flee()
        {
            var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
            var randomManager = new RandomManager();
            var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
            var roomMock = new Mock<IRoom>();
            var fleeRoomMock = new Mock<IRoom>();
            var pc = GeneratePC("pc", aggroManager, roomMock.Object);
            var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns([pc, npc]);

            aggroManager.OnStartFight(pc, npc);
            aggroManager.OnReceiveDamage(pc, npc, 4 * 10);
            aggroManager.OnFlee(pc, roomMock.Object);
            pc.ChangeRoom(fleeRoomMock.Object, false);

            var startFightAggro = 1;
            var damageAggro = startFightAggro + 4 * 10;
            var fleeAggro = damageAggro - Math.Max(1, damageAggro / 3); // when fleeing remove 1/3 of aggro

            Assert.AreEqual(fleeAggro, aggroManager.GetAggroTable(npc)!.AggroByEnemy[pc]);
        }

        [TestMethod]
        public void Flee_AdditionalRound()
        {
            var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
            var randomManager = new RandomManager();
            var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
            var roomMock = new Mock<IRoom>();
            var fleeRoomMock = new Mock<IRoom>();
            var pc = GeneratePC("pc", aggroManager, roomMock.Object);
            var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns([pc, npc]);

            aggroManager.OnStartFight(pc, npc);
            aggroManager.OnReceiveDamage(pc, npc, 4 * 10);
            aggroManager.OnFlee(pc, roomMock.Object);
            pc.ChangeRoom(fleeRoomMock.Object, false);
            aggroManager.DecreaseAggroOfEnemiesIfNotInSameRoom(npc);

            var startFightAggro = 1;
            var damageAggro = startFightAggro + 4 * 10;
            var fleeAggro = damageAggro - Math.Max(1, damageAggro / 3); // when fleeing remove 1/3 of aggro
            var finalAggro = fleeAggro - Math.Max(10, fleeAggro / 10); // each round out of combat remove 10% (with min of 10)
            Assert.AreEqual(finalAggro, aggroManager.GetAggroTable(npc)!.AggroByEnemy[pc]);
        }

        [TestMethod]
        public void Flee_AdditionalRound_ReturnsBack()
        {
            var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
            var randomManager = new RandomManager();
            var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
            var roomMock = new Mock<IRoom>();
            var fleeRoomMock = new Mock<IRoom>();
            var pc = GeneratePC("pc", aggroManager, roomMock.Object);
            var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns([pc, npc]);

            aggroManager.OnStartFight(pc, npc);
            aggroManager.OnReceiveDamage(pc, npc, 4 * 10);
            aggroManager.OnFlee(pc, roomMock.Object);
            pc.ChangeRoom(fleeRoomMock.Object, false);
            aggroManager.DecreaseAggroOfEnemiesIfNotInSameRoom(npc);
            pc.ChangeRoom(roomMock.Object, false);
            aggroManager.OnReceiveDamage(pc, npc, 4 * 10);

            var startFightAggro = 1;
            var damageAggro = startFightAggro + 4 * 10;
            var fleeAggro = damageAggro - Math.Max(1, damageAggro / 3); // when fleeing remove 1/3 of aggro
            var inFleeRoomAggro = fleeAggro - Math.Max(10, fleeAggro / 10); // each round out of combat remove 10% (with min of 10)
            var finalAggro = inFleeRoomAggro + 4 * 10; // one round of autoattack
            Assert.AreEqual(finalAggro, aggroManager.GetAggroTable(npc)!.AggroByEnemy[pc]);
        }
    }
}
