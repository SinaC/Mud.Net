using Mud.POC.Combat;

namespace Mud.POC.Tests.Combat;

// TODO: same stuff but with npc performing kill command

[TestClass]
public class StartCombatWithKillCommandTests
{
    private static void KillCommand(ICombatManager combatManager, ICharacter character, ICharacter victim)
    {
        combatManager.StartCombat(character, victim);
    }

    [TestMethod]
    public void PcStartsFightingNpc()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);

        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100);
        var npc = new NonPlayableCharacter(combatManager, "npc", 100);

        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);

        KillCommand(combatManager, pc, npc);

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.IsNotNull(combatManager.GetFighting(npc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }

    [TestMethod]
    public void PcPet_PcStartsFightingNpc()
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

        KillCommand(combatManager, pc, npc);

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pet));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pet]); // 1 from joining combat
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pet));
        Assert.IsNotNull(combatManager.GetFighting(npc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pet.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }

    [TestMethod]
    public void PcPet_PetStartsFightingNpc()
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

        KillCommand(combatManager, pet, npc);

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pet));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pet]); // 1 from joining combat
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pet));
        Assert.IsNotNull(combatManager.GetFighting(npc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pet.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }

    [TestMethod]
    public void PcGroup_LeaderStartsFightingNpc()
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

        KillCommand(combatManager, pc, npc);

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc2));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc2]); // 1 from joining combat
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc2));
        Assert.IsNotNull(combatManager.GetFighting(npc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pc2.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }

    [TestMethod]
    public void PcGroup_MemberStartsFightingNpc()
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

        KillCommand(combatManager, pc2, npc);

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc2));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc2]); // 1 from joining combat
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc2));
        Assert.IsNotNull(combatManager.GetFighting(npc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pc2.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }
}
