using Microsoft.Extensions.Logging;
using Moq;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Combat;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Tests.Aggro;

[TestClass]
public class HealTests : CombatTestsBase
{
    [TestMethod]
    public void PcPet_PcDealsDamagePetHealsALot()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        roomMock.SetupGet(x => x.RoomFlags).Returns(new RoomFlags());
        var pc1 = GeneratePC("pc", aggroManager, roomMock.Object);
        var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
        var pet = GenerateNPC("pet", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc1, npc, pet]);
        pet.ActFlags.Set("pet");
        pet.CharacterFlags.Set("charm");
        pc1.AddPet(pet);

        pc1.StartFighting(npc);
        pet.StartFighting(npc);
        npc.AbilityDamage(pc1, 30, Mud.Domain.SchoolTypes.None, null, false);
        pc1.Heal(pet, 1000);

        Assert.IsTrue(aggroManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc1));
        Assert.AreEqual(1+30, aggroManager.GetAggroTable(npc)?.AggroByEnemy[pc1]); // 30 from damage and 1 from start combat
        Assert.IsTrue(aggroManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pet));
        Assert.AreEqual(1+1000/4, aggroManager.GetAggroTable(npc)?.AggroByEnemy[pet]); // 1000/4 from heal and 1 from start combat
        Assert.AreEqual(pet, aggroManager.GetAggroTable(npc)?.GetEnemyInRoom(npc.Room!));
    }

    [TestMethod]
    public void PcGroup_LeaderDealsDamageMemberHealsALot()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        roomMock.SetupGet(x => x.RoomFlags).Returns(new RoomFlags());
        var pc1 = GeneratePC("pc1", aggroManager, roomMock.Object);
        var pc2 = GeneratePC("pc2", aggroManager, roomMock.Object);
        var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc1, pc2, npc]);
        // create a group with pc1 as leader and pc2 as member
        var group = new Group.Group(pc1);
        pc2.ChangeGroup(group);

        pc1.StartFighting(npc);
        pc2.StartFighting(npc);
        npc.AbilityDamage(pc1, 30, Mud.Domain.SchoolTypes.None, null, false);
        pc1.Heal(pc2, 1000);

        Assert.IsTrue(aggroManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc1));
        Assert.AreEqual(1 + 30, aggroManager.GetAggroTable(npc)?.AggroByEnemy[pc1]); // 30 from damage and 1 from start combat
        Assert.IsTrue(aggroManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc2));
        Assert.AreEqual(1 + 1000 / 4, aggroManager.GetAggroTable(npc)?.AggroByEnemy[pc2]); // 1000/4 from heal and 1 from start combat
        Assert.AreEqual(pc2, aggroManager.GetAggroTable(npc)?.GetEnemyInRoom(npc.Room!));
    }

    [TestMethod]
    public void PcFightsNpc_AnotherPcNotInCombatHealsPc()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        roomMock.SetupGet(x => x.RoomFlags).Returns(new RoomFlags());
        var pc1 = GeneratePC("pc1", aggroManager, roomMock.Object);
        var pc2 = GeneratePC("pc2", aggroManager, roomMock.Object);
        var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc1, pc2, npc]);

        pc1.StartFighting(npc);
        npc.AbilityDamage(pc1, 30, Mud.Domain.SchoolTypes.None, null, false);
        pc1.Heal(pc2, 1000);

        Assert.IsTrue(aggroManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc1));
        Assert.AreEqual(1 + 30, aggroManager.GetAggroTable(npc)?.AggroByEnemy[pc1]); // 30 from damage and 1 from start combat
        Assert.IsTrue(aggroManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc2));
        Assert.AreEqual(1000 / 4, aggroManager.GetAggroTable(npc)?.AggroByEnemy[pc2]); // 1000/4 from heal
        Assert.AreEqual(pc2, aggroManager.GetAggroTable(npc)?.GetEnemyInRoom(npc.Room!));
    }
}
