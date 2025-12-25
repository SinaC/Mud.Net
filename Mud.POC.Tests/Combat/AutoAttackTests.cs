using Mud.POC.Combat;

namespace Mud.POC.Tests.Combat;

[TestClass]
public class AutoAttackTests
{
    [TestMethod]
    public void OneRound_NoDeath()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);
        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100)
        {
            AutoAttackCount = 4,
            AutoAttachDamage = 10
        };
        var npc = new NonPlayableCharacter(combatManager, "npc", 100)
        {
            AutoAttackCount = 3,
            AutoAttachDamage = 20
        };
        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);

        combatManager.StartCombat(pc, npc);
        server.HandleCombatRound();

        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.AreEqual(100 - 3 * 20, pc.HitPoints);
        Assert.AreEqual(100 - 4 * 10, npc.HitPoints);
        Assert.AreEqual(pc, combatManager.GetFighting(npc));
        Assert.AreEqual(pc, combatManager.GetAggroTable(npc)!.GetEnemyInRoom(npc.Room!));
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.IsEmpty(room.Content.OfType<IItemCorpse>());
    }

    [TestMethod]
    public void OneRound_PcDead()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);
        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100)
        {
            AutoAttackCount = 4,
            AutoAttachDamage = 10,
            Initiative = 1, // will hit before npc
        };
        var npc = new NonPlayableCharacter(combatManager, "npc", 100)
        {
            AutoAttackCount = 6,
            AutoAttachDamage = 22,
            Initiative = 10, // will hit after pc
        };
        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);

        combatManager.StartCombat(pc, npc);
        server.HandleCombatRound();

        Assert.IsFalse(pc.IsDead); // after each combat round, npc are removed and pc are moved to death room and reset to 1
        Assert.IsFalse(npc.IsDead);
        Assert.AreEqual(1, pc.HitPoints);
        Assert.AreEqual(100 - 4 * 10, npc.HitPoints);
        Assert.AreEqual(Room.DeathRoom, pc.Room);
        Assert.AreEqual(room, npc.Room);
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of pc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void OneRound_NpcDead()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);
        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100)
        {
            AutoAttackCount = 7,
            AutoAttachDamage = 22,
            Initiative = 10, // will hit after npc
        };
        var npc = new NonPlayableCharacter(combatManager, "npc", 100)
        {
            AutoAttackCount = 3,
            AutoAttachDamage = 20,
            Initiative = 1 // will hit first
        };
        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);

        combatManager.StartCombat(pc, npc);
        server.HandleCombatRound();

        Assert.IsFalse(pc.IsDead); // after each combat round, npc are removed and pc are moved to death room and reset to 1
        Assert.IsTrue(npc.IsDead);
        Assert.AreEqual(100 - 3 * 20, pc.HitPoints);
        Assert.AreEqual(100 - 5 * 22, npc.HitPoints);
        Assert.AreEqual(room, pc.Room);
        Assert.IsNull(npc.Room);
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of npc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void OneRound_PcDeadOnNpcCounterAttack()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);
        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100)
        {
            AutoAttackCount = 4,
            AutoAttachDamage = 10,
            Initiative = 1, // will hit before npc
        };
        var npc = new NonPlayableCharacter(combatManager, "npc", 100)
        {
            AutoAttackCount = 3,
            AutoAttachDamage = 20,
            IsCounterAttackActive = true,
            CounterAttackDamage = 100,
            Initiative = 10, // will hit after pc
        };
        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);

        combatManager.StartCombat(pc, npc);
        server.HandleCombatRound();

        Assert.IsFalse(pc.IsDead); // after each combat round, npc are removed and pc are moved to death room and reset to 1
        Assert.IsFalse(npc.IsDead);
        Assert.AreEqual(1, pc.HitPoints);
        Assert.AreEqual(100 - 1 * 10, npc.HitPoints); // counter attack killed pc after 1st hit
        Assert.AreEqual(Room.DeathRoom, pc.Room);
        Assert.AreEqual(room, npc.Room);
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of pc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void OneRound_NpcDeadBeforeItsCounterAttack()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);
        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100)
        {
            AutoAttackCount = 4,
            AutoAttachDamage = 150,
            Initiative = 1, // will hit before npc
        };
        var npc = new NonPlayableCharacter(combatManager, "npc", 100)
        {
            AutoAttackCount = 3,
            AutoAttachDamage = 20,
            IsCounterAttackActive = true,
            CounterAttackDamage = 100,
            Initiative = 10, // will hit after pc
        };
        server.AddPlayableCharacter(pc, room);
        server.AddNonPlayableCharacter(npc, room);

        combatManager.StartCombat(pc, npc);
        server.HandleCombatRound();

        Assert.IsFalse(pc.IsDead);
        Assert.IsTrue(npc.IsDead);
        Assert.AreEqual(100, pc.HitPoints);
        Assert.AreEqual(100 - 1 * 150, npc.HitPoints); // npc killed after first pc's hit
        Assert.AreEqual(room, pc.Room);
        Assert.IsNull(npc.Room);
        Assert.ContainsSingle(room.Content.OfType<IItemCorpse>());
        Assert.AreEqual("the corpse of npc", room.Content.OfType<IItemCorpse>().Single().CorpseName);
    }

    [TestMethod]
    public void TwoRounds_Group_AggroSwitch()
    {
        var combatManager = new CombatManager();
        var server = new POC.Combat.Server(combatManager);

        var room = new Room("room1");
        var pc = new PlayableCharacter(combatManager, "pc", 100)
        {
            AutoAttackCount = 4,
            AutoAttachDamage = 10,
            Initiative = 1, // will hit first
        };
        var pc2 = new PlayableCharacter(combatManager, "pc2", 100)
        {
            AutoAttackCount = 1,
            AutoAttachDamage = 10,
            Initiative = 2, // will hit second
        };
        var npc = new NonPlayableCharacter(combatManager, "npc", 1000)
        {
            AutoAttackCount = 1,
            AutoAttachDamage = 45,
            Initiative = 3, // will hit third
        };

        server.AddPlayableCharacter(pc, room);
        server.AddPlayableCharacter(pc2, room);
        server.AddNonPlayableCharacter(npc, room);
        pc.JoinGroup(pc2);

        combatManager.StartCombat(pc, npc);
        server.HandleCombatRound();

        // change pc2 to hit harder
        pc2.AutoAttachDamage = 100;

        // an additional round, pc2 should be first in aggro list after this round
        server.HandleCombatRound();

        Assert.IsFalse(pc.IsDead);
        Assert.IsFalse(pc2.IsDead);
        Assert.IsFalse(npc.IsDead);
        Assert.AreEqual(100 - 1 * 45, pc.HitPoints); // hit once by npc in round1
        Assert.AreEqual(100 - 1 * 45, pc2.HitPoints); // hit once by npc in round2
        Assert.AreEqual(1000 - 4 * 10 - 1 * 10 - 4 * 10 - 1 * 100, npc.HitPoints); // hit once by pc1, once by pc2, a second time by pc1, a second time by pc2
        Assert.AreEqual(pc2, combatManager.GetFighting(npc));
        Assert.AreEqual(pc2, combatManager.GetAggroTable(npc)!.GetEnemyInRoom(npc.Room!));
        Assert.AreEqual(npc, combatManager.GetFighting(pc));
        Assert.AreEqual(npc, combatManager.GetFighting(pc2));
    }
}
