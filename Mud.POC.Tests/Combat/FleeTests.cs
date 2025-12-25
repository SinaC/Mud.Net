using Mud.POC.Combat;

namespace Mud.POC.Tests.Combat
{
    [TestClass]
    public class FleeTests
    {
        [TestMethod]
        public void Flee()
        {
            var combatManager = new CombatManager();
            var server = new POC.Combat.Server(combatManager);
            var room = new Room("room1");
            var pc = new PlayableCharacter(combatManager, "pc", 100)
            {
                AutoAttackCount = 4,
                AutoAttachDamage = 10,
                Initiative = 2, // will hit after npc
            };
            var npc = new NonPlayableCharacter(combatManager, "npc", 100)
            {
                AutoAttackCount = 3,
                AutoAttachDamage = 20,
                Initiative = 1, // will hit before pc
            };
            server.AddPlayableCharacter(pc, room);
            server.AddNonPlayableCharacter(npc, room);

            combatManager.StartCombat(pc, npc);
            server.HandleCombatRound();
            combatManager.FleeCombat(pc);

            var joinCombatAggro = 1;
            var damageAggro = joinCombatAggro + 4 * 10;
            var fleeAggro = damageAggro - Math.Max(1, damageAggro / 3); // when fleeing remove 1/3 of aggro
            Assert.IsFalse(pc.IsDead);
            Assert.IsFalse(npc.IsDead);
            Assert.IsNull(combatManager.GetFighting(pc));
            Assert.IsNull(combatManager.GetFighting(npc));
            Assert.AreEqual(Room.FleeRoom, pc.Room);
            Assert.AreEqual(room, npc.Room);
            Assert.AreEqual(fleeAggro, combatManager.GetAggroTable(npc)!.AggroByEnemy[pc]);
        }

        [TestMethod]
        public void Flee_AdditionalRound()
        {
            var combatManager = new CombatManager();
            var server = new POC.Combat.Server(combatManager);
            var room = new Room("room1");
            var pc = new PlayableCharacter(combatManager, "pc", 100)
            {
                AutoAttackCount = 4,
                AutoAttachDamage = 10,
                Initiative = 2, // will hit after npc
            };
            var npc = new NonPlayableCharacter(combatManager, "npc", 100)
            {
                AutoAttackCount = 3,
                AutoAttachDamage = 20,
                Initiative = 1, // will hit before pc
            };
            server.AddPlayableCharacter(pc, room);
            server.AddNonPlayableCharacter(npc, room);

            combatManager.StartCombat(pc, npc);
            server.HandleCombatRound();
            combatManager.FleeCombat(pc);
            server.HandleCombatRound();

            var joinCombatAggro = 1;
            var damageAggro = joinCombatAggro + 4 * 10;
            var fleeAggro = damageAggro - Math.Max(1, damageAggro / 3); // when fleeing remove 1/3 of aggro
            var finalAggro = fleeAggro - Math.Max(1, fleeAggro / 10); // each round out of combat remove 10%
            Assert.IsFalse(pc.IsDead);
            Assert.IsFalse(npc.IsDead);
            Assert.IsNull(combatManager.GetFighting(pc));
            Assert.IsNull(combatManager.GetFighting(npc));
            Assert.AreEqual(Room.FleeRoom, pc.Room);
            Assert.AreEqual(room, npc.Room);
            Assert.AreEqual(finalAggro, combatManager.GetAggroTable(npc)!.AggroByEnemy[pc]); 
        }

        [TestMethod]
        public void Flee_AdditionalRound_ReturnsBack()
        {
            var combatManager = new CombatManager();
            var server = new POC.Combat.Server(combatManager);
            var room = new Room("room1");
            var pc = new PlayableCharacter(combatManager, "pc", 1000)
            {
                AutoAttackCount = 4,
                AutoAttachDamage = 10,
                Initiative = 2, // will hit after npc
            };
            var npc = new NonPlayableCharacter(combatManager, "npc", 1000)
            {
                AutoAttackCount = 3,
                AutoAttachDamage = 20,
                Initiative = 1, // will hit before pc
            };
            server.AddPlayableCharacter(pc, room);
            server.AddNonPlayableCharacter(npc, room);

            combatManager.StartCombat(pc, npc);
            server.HandleCombatRound();
            combatManager.FleeCombat(pc);
            server.HandleCombatRound();
            pc.Room?.Remove(pc);
            pc.SetRoom(room);
            server.HandleCombatRound();

            var joinCombatAggro = 1;
            var damageAggro = joinCombatAggro + 4 * 10;
            var fleeAggro = damageAggro - Math.Max(1, damageAggro / 3); // when fleeing remove 1/3 of aggro
            var inFleeRoomAggro = fleeAggro - Math.Max(1, fleeAggro / 10); // each round out of combat remove 10%
            var finalAggro = inFleeRoomAggro + joinCombatAggro + 4 * 10; // re-join combat + one round of autoaack
            Assert.IsFalse(pc.IsDead);
            Assert.IsFalse(npc.IsDead);
            Assert.IsNotNull(combatManager.GetFighting(pc));
            Assert.IsNotNull(combatManager.GetFighting(npc));
            Assert.AreEqual(room, pc.Room);
            Assert.AreEqual(room, npc.Room);
            Assert.AreEqual(finalAggro, combatManager.GetAggroTable(npc)!.AggroByEnemy[pc]);
        }

        [TestMethod]
        public void Flee_AdditionalRound_ReturnsBack_Killed()
        {
            var combatManager = new CombatManager();
            var server = new POC.Combat.Server(combatManager);
            var room = new Room("room1");
            var pc = new PlayableCharacter(combatManager, "pc", 100)
            {
                AutoAttackCount = 4,
                AutoAttachDamage = 10,
                Initiative = 2, // will hit after npc
            };
            var npc = new NonPlayableCharacter(combatManager, "npc", 100)
            {
                AutoAttackCount = 3,
                AutoAttachDamage = 20,
                Initiative = 1, // will hit before pc
            };
            server.AddPlayableCharacter(pc, room);
            server.AddNonPlayableCharacter(npc, room);

            combatManager.StartCombat(pc, npc);
            server.HandleCombatRound();
            combatManager.FleeCombat(pc);
            server.HandleCombatRound();
            pc.Room?.Remove(pc);
            pc.SetRoom(room);
            server.HandleCombatRound();

            Assert.IsFalse(pc.IsDead);
            Assert.IsFalse(npc.IsDead);
            Assert.IsNull(combatManager.GetFighting(pc));
            Assert.IsNull(combatManager.GetFighting(npc));
            Assert.AreEqual(Room.DeathRoom, pc.Room);
            Assert.AreEqual(room, npc.Room);
            Assert.IsNotNull(combatManager.GetAggroTable(npc));
            Assert.IsEmpty(combatManager.GetAggroTable(npc)!.AggroByEnemy);
        }
    }
}
