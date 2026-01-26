using Mud.POC.Combat;

namespace Mud.POC.Tests.Combat;

[TestClass]
public class HealTests
{
    [TestMethod]
    public void PcPet_PcDealsDamagePetHealsALot()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);

        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100);
        var npc = new NonPlayableCharacter(combatManager, "npc", 100);
        var pet = new NonPlayableCharacter(combatManager, "pet", 100);

        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);
        server.AddNonPlayableCharacter(pet, room);
        pc.AddPet(pet);

        combatManager.AbilityDamage(pc, npc, 50, Domain.SchoolTypes.Bash);
        combatManager.Heal(pet, pc, 1000);
        server.HandleCombatRound(); // npc top enemy is chosen at the beginning of each combat round

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1+50, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 50 from damage
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pet));
        Assert.AreEqual(1+(1000/4), combatManager.GetAggroTable(npc)?.AggroByEnemy[pet]); // 1000/4 from heal
        Assert.AreEqual(pet, combatManager.GetAggroTable(npc)?.GetEnemyInRoom(npc.Room!));
        Assert.AreEqual(pet, combatManager.GetFighting(npc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pet));
    }

    [TestMethod]
    public void PcGroup_LeaderDealsDamageMemberHealsALot()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);

        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100);
        var pc2 = new PlayableCharacter(combatManager, "pc2", 100);
        var npc = new NonPlayableCharacter(combatManager, "npc", 100);

        server.AddPlayableCharacter(pc, room);
        server.AddPlayableCharacter(pc2, room);
        server.AddNonPlayableCharacter(npc, room);
        pc.JoinGroup(pc2);

        combatManager.AbilityDamage(pc, npc, 50, Domain.SchoolTypes.Bash);
        combatManager.Heal(pc2, pc, 1000);
        server.HandleCombatRound(); // npc top enemy is chosen at the beginning of each combat round

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1+50, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 50 from damage
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc2));
        Assert.AreEqual(1+(1000 / 4), combatManager.GetAggroTable(npc)?.AggroByEnemy[pc2]); // 1000/4 from heal
        Assert.AreEqual(pc2, combatManager.GetAggroTable(npc)?.GetEnemyInRoom(npc.Room!));
        Assert.AreEqual(pc2, combatManager.GetFighting(npc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc2));
    }

    [TestMethod]
    public void PcFightsNpc_AnotherPcNotInCombatHealsPc()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);

        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100);
        var pc2 = new PlayableCharacter(combatManager, "pc2", 100);
        var npc = new NonPlayableCharacter(combatManager, "npc", 100) { AutoAttachDamage = 10, AutoAttackCount = 1 }; // needed to include pc2 in the fight after heal + handle combat round

        server.AddPlayableCharacter(pc, room);
        server.AddPlayableCharacter(pc2, room);
        server.AddNonPlayableCharacter(npc, room);

        combatManager.AbilityDamage(pc, npc, 50, Domain.SchoolTypes.Bash);
        combatManager.Heal(pc2, pc, 1000);
        server.HandleCombatRound(); // npc top enemy is chosen at the beginning of each combat round

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1 + 50, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat and 50 from damage
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc2));
        Assert.AreEqual(1000 / 4 + 1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc2]); // 1000/4 from heal and 1 from joining combat
        Assert.AreEqual(pc2, combatManager.GetAggroTable(npc)?.GetEnemyInRoom(npc.Room!));
        Assert.AreEqual(pc2, combatManager.GetFighting(npc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc2));
    }
}
