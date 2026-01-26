using Mud.POC.Combat;

namespace Mud.POC.Tests.Combat;

// TODO: groups

[TestClass]
public class DeathOnFirstHitTests
{
    [TestMethod]
    public void PcKillsNpc()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);

        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100);
        var npc = new NonPlayableCharacter(combatManager, "npc", 100);

        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);

        combatManager.AbilityDamage(pc, npc, 1000, Domain.SchoolTypes.Bash);

        Assert.IsNull(combatManager.GetAggroTable(npc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsTrue(npc.IsDead);
        Assert.IsNull(combatManager.GetFighting(pc));
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of npc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void NpcKillsPc()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);

        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100);
        var npc = new NonPlayableCharacter(combatManager, "npc", 100);

        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);

        combatManager.AbilityDamage(npc, pc, 1000, Domain.SchoolTypes.Bash);

        Assert.IsEmpty(combatManager.GetAggroTable(npc)?.AggroByEnemy!); // pc was killed on first hit then pc was added then removed from aggro table
        Assert.IsTrue(pc.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsNull(combatManager.GetFighting(npc));
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of pc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void PcPet_PcKillsNpc()
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

        combatManager.AbilityDamage(pc, npc, 1000, Domain.SchoolTypes.Bash);

        Assert.IsFalse(pc.IsDead);
        Assert.IsTrue(npc.IsDead);
        Assert.IsFalse(pet.IsDead);
        Assert.IsNull(combatManager.GetFighting(pc));
        Assert.IsNull(combatManager.GetFighting(npc));
        Assert.IsNull(combatManager.GetFighting(pet));
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of npc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void PcPet_NpcKillsPet()
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

        // npc deals lethal damage to pet who has pc as master
        //      npc starts a fight with pet
        //      pet and pc starts a fight with npc
        //      npc deals lethal damage to pet
        //      pet dies and is removed from fight
        //      npc doesn't fight anyone anymore
        //      pc is still fighting npc
        combatManager.AbilityDamage(npc, pet, 1000, Domain.SchoolTypes.Bash);

        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsTrue(pet.IsDead);
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.IsNull(combatManager.GetFighting(pet));
        Assert.IsNull(combatManager.GetFighting(npc));
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of pet", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void PcGroup_NpcKillsMember()
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

        // npc deals lethal damage to member who has pc as master
        //      npc starts a fight with member
        //      member and pc starts a fight with npc
        //      npc deals lethal damage to member
        //      member dies and is removed from fight
        //      npc doesn't fight anyone anymore
        //      pc is still fighting npc
        combatManager.AbilityDamage(npc, pc2, 1000, Domain.SchoolTypes.Bash);

        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsTrue(pc2.IsDead);
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.IsNull(combatManager.GetFighting(pc2));
        Assert.IsNull(combatManager.GetFighting(npc));
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of pc2", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }
}
