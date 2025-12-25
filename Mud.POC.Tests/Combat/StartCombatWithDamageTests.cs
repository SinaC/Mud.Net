using Mud.POC.Combat;

namespace Mud.POC.Tests.Combat;

// TODO: same stuff but with npc dealing damage

[TestClass]
public class StartCombatWithDamageTests
{
    [TestMethod]
    public void PcHitsNpc()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);

        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100);
        var npc = new NonPlayableCharacter(combatManager, "npc", 100);

        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);

        combatManager.AbilityDamage(pc, npc, 50, Domain.SchoolTypes.Bash);

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1 + 50, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat and 50 from damage
        Assert.AreEqual(pc, combatManager.GetAggroTable(npc)?.GetEnemyInRoom(npc.Room!));
        Assert.AreEqual(pc, combatManager.GetFighting(npc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }

    [TestMethod]
    public void PcPet_PcHitsNpc()
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

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1 + 50, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat and 50 from damage
        Assert.AreEqual(pc, combatManager.GetAggroTable(npc)?.GetEnemyInRoom(npc.Room!));
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pet));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pet]); // 1 from joining combat
        Assert.AreEqual(pc, combatManager.GetFighting(npc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pet));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pet.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }

    [TestMethod]
    public void PcPet_PetHitsNpc()
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

        combatManager.AbilityDamage(pet, npc, 50, Domain.SchoolTypes.Bash);

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pet));
        Assert.AreEqual(1 + 50, combatManager.GetAggroTable(npc)?.AggroByEnemy[pet]); // 1 from joining combat + 50 from damage
        Assert.AreEqual(pet, combatManager.GetAggroTable(npc)?.GetEnemyInRoom(npc.Room!));
        Assert.AreEqual(pet, combatManager.GetFighting(npc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pet));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pet.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }

    [TestMethod]
    public void PcGroup_LeaderHitsNpc()
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

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1 + 50, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat and 50 from damage
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc2));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc2]); // 1 from joining combat
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc2));
        Assert.AreEqual(pc, combatManager.GetFighting(npc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pc2.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }

    [TestMethod]
    public void PcGroup_MemberHitsNpc()
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

        combatManager.AbilityDamage(pc2, npc, 50, Domain.SchoolTypes.Bash);

        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc));
        Assert.AreEqual(1, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc]); // 1 from joining combat
        Assert.IsTrue(combatManager.GetAggroTable(npc)?.AggroByEnemy.ContainsKey(pc2));
        Assert.AreEqual(1 + 50, combatManager.GetAggroTable(npc)?.AggroByEnemy[pc2]); // 1 from joining combat and 50 from damage
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc2));
        Assert.AreEqual(pc2, combatManager.GetFighting(npc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pc2.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }
}
