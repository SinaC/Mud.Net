namespace Mud.POC.Combat;

public class Server
{
    private ICombatManager CombatManager { get; }

    private readonly List<ICharacter> _characters = [];

    public Server(ICombatManager combatManager)
    {
        CombatManager = combatManager;
    }

    public void AddPlayableCharacter(IPlayableCharacter pc, IRoom room)
    {
        _characters.Add(pc);
        pc.SetRoom(room);
        room.Add(pc);
    }

    public void AddNonPlayableCharacter(INonPlayableCharacter mob, IRoom room)
    {
        _characters.Add(mob);
        mob.SetRoom(room);
        room.Add(mob);
    }

    public void HandleCombatRound()
    {
        // stop combat if not in same room
        foreach (var ch in _characters)
        {
            var fighting = CombatManager.GetFighting(ch);
            if (fighting != null && fighting.Room != ch.Room)
                CombatManager.StopFighting(ch);
        }

        // initiate one round of combat
        foreach (var ch in _characters.OrderBy(x => x.Initiative))
        {
            if (ch is INonPlayableCharacter npc && !npc.IsDead)
                CombatManager.CombatRound(npc);
            else if (ch is IPlayableCharacter pc && !pc.IsDead)
                CombatManager.CombatRound(pc);
        }
        // TODO: auto assist for mob

        // remove dead NPC and resurrect PC
        HandleDeadCharacters();
        // decrease aggro if not in same room
        foreach (var npc in _characters.OfType<INonPlayableCharacter>())
            CombatManager.DecreaseAggroOfEnemiesIfNotInSameRoom(npc);
    }

    private void HandleDeadCharacters()
    {
        // delete every dead NPC
        _characters.RemoveAll(x => x is INonPlayableCharacter && x.IsDead);

        // unflag dead for PC
        var pcToRecompute = new HashSet<IPlayableCharacter>();
        foreach (var pc in _characters.OfType<IPlayableCharacter>().Where(x => x.IsDead))
        {
            pc.RemoveDeadFlag();
            pc.SetHitPoints(1);

            // TODO
            // reset base auras
            // set position to resting
            // recompute
            pcToRecompute.Add(pc);
        }

        // recompute PCs
        foreach (var pc in pcToRecompute)
            pc.Recompute();
    }
}
