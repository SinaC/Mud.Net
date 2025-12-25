using Mud.POC.Combat;
using Mud.Server.Combat;
using Mud.Server.Interfaces.Combat;

namespace Mud.POC.Tests.Combat;

[TestClass]
public class DeathDuringCombatTests
{
    private static void KillCommand(ICombatManager combatManager, ICharacter character, ICharacter victim)
    {
        combatManager.StartCombat(character, victim);
    }

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

        KillCommand(combatManager, pc, npc);
        server.HandleCombatRound();
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

        KillCommand(combatManager, pc, npc);
        server.HandleCombatRound();
        combatManager.AbilityDamage(npc, pc, 1000, Domain.SchoolTypes.Bash);

        Assert.IsEmpty(combatManager.GetAggroTable(npc)!.AggroByEnemy.Keys);
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

        KillCommand(combatManager, pc, npc);
        server.HandleCombatRound();
        combatManager.AbilityDamage(pc, npc, 1000, Domain.SchoolTypes.Bash);

        Assert.IsFalse(pc.IsDead);
        Assert.IsTrue(npc.IsDead);
        Assert.IsNull(combatManager.GetFighting(pc));
        Assert.IsNull(combatManager.GetFighting(pet));
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of npc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void Group_LeaderKillsNpc()
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
        server.HandleCombatRound();
        combatManager.AbilityDamage(pc, npc, 1000, Domain.SchoolTypes.Bash);

        Assert.IsNull(combatManager.GetAggroTable(npc));
        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pc2.IsDead);
        Assert.IsTrue(npc.IsDead);
        Assert.IsNull(combatManager.GetFighting(pc));
        Assert.IsNull(combatManager.GetFighting(pc2));
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of npc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void Group_NpcKillsLeader()
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
        server.HandleCombatRound();
        combatManager.AbilityDamage(npc, pc, 1000, Domain.SchoolTypes.Bash);

        Assert.IsNotNull(combatManager.GetAggroTable(npc));
        Assert.IsNotEmpty(combatManager.GetAggroTable(npc)!.AggroByEnemy);
        Assert.AreEqual(pc2, combatManager.GetAggroTable(npc)!.GetEnemyInRoom(npc.Room!));
        Assert.IsTrue(pc.IsDead);
        Assert.IsFalse(pc2.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsNull(combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc2));
        Assert.IsNull(combatManager.GetFighting(npc)); // npc will select pc2 during next round
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of pc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void Group_NpcKillsLeader_AdditionalRound()
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
        server.HandleCombatRound();
        combatManager.AbilityDamage(npc, pc, 1000, Domain.SchoolTypes.Bash);
        server.HandleCombatRound();

        Assert.IsNotNull(combatManager.GetAggroTable(npc));
        Assert.IsNotEmpty(combatManager.GetAggroTable(npc)!.AggroByEnemy);
        Assert.AreEqual(pc2, combatManager.GetAggroTable(npc)!.GetEnemyInRoom(npc.Room!));
        Assert.IsFalse(pc.IsDead); // pc has been automatically resurrected
        Assert.IsFalse(pc2.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.IsNull(combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc2));
        Assert.AreEqual(pc2, combatManager.GetFighting(npc));
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of pc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }
}
