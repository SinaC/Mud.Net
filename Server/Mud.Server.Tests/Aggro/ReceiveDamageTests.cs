using Microsoft.Extensions.Logging;
using Moq;
using Mud.Random;
using Mud.Server.Combat;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Tests.Aggro;

[TestClass]
public class ReceiveDamageTests : CombatTestsBase
{
    [TestMethod]
    public void PC_Vs_NPC()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        var pc = GeneratePC("pc", aggroManager, roomMock.Object);
        var npc = GenerateNPC("npc", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc, npc]);

        aggroManager.OnStartFight(pc, npc);
        aggroManager.OnReceiveDamage(pc, npc, 4 * 10);

        var joinCombatAggro = 1;
        var damageAggro = joinCombatAggro + 4 * 10;

        Assert.AreEqual(damageAggro, aggroManager.GetAggroTable(npc)!.AggroByEnemy[pc]);
    }

    [TestMethod]
    public void PC_Vs_Pet()
    {
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 10, UseAggro = true, BlueprintIds = new BlueprintIds { Coins = 0, Corpse = 0, DefaultDeathRoom = 0, DefaultRecallRoom = 0, DefaultRoom = 0, MudSchoolRoom = 0, NullRoom = 0 } });
        var randomManager = new RandomManager();
        var aggroManager = new AggroManager(new Mock<ILogger<AggroManager>>().Object, worldOptions);
        var roomMock = new Mock<IRoom>();
        var pc = GeneratePC("pc", aggroManager, roomMock.Object);
        var pet = GenerateNPC("pet", randomManager, aggroManager, roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([pc, pet]);
        pet.ActFlags.Set("pet");
        pet.CharacterFlags.Set("charm");

        aggroManager.OnStartFight(pc, pet);
        aggroManager.OnReceiveDamage(pc, pet, 4 * 10);

        Assert.IsNull(aggroManager.GetAggroTable(pet)); // no aggro table for pet
    }
}
