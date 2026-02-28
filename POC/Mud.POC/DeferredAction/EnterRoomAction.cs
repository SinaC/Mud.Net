namespace Mud.POC.DeferredAction;

public class EnterRoomAction : IGameAction
{
    private readonly Mob _mob;
    private readonly Room _targetRoom;

    public EnterRoomAction(Mob mob, Room targetRoom)
    {
        _mob = mob;
        _targetRoom = targetRoom;
    }

    public void Execute(World world)
    {
        if (_mob.IsDead) return;

        world.ScheduleMutation(() =>
        {
            _mob.CurrentRoom?.Mobs.Remove(_mob);
            _targetRoom.Mobs.Add(_mob);
            _mob.CurrentRoom = _targetRoom;

            // Now that room is correct, enqueue look
            world.Enqueue(new LookRoomAction(_mob));
        });

        foreach (var follower in world.AllMobsInWorld()
         .Where(m => m.Master == _mob && m.IsCharmed))
        {
            world.Enqueue(new EnterRoomAction(follower, _targetRoom));
        }

        // Trigger aggro (snapshot safety: room state still valid)
        foreach (var other in _targetRoom.Mobs)
        {
            if (ShouldAttack(other, _mob))
                world.Enqueue(new AttackAction(other, _mob));
        }
    }

    private bool ShouldAttack(Mob attacker, Mob target)
        => !attacker.IsDead;
}
