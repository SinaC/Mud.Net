using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class FleeAction : IGameAction
{
    private readonly Mob _mob;

    public FleeAction(Mob mob)
    {
        _mob = mob;
    }

    public void Execute(World world)
    {
        if (!_mob.CanAct)
            return;

        var room = _mob.CurrentRoom;
        if (room.Exits.Count == 0)
            return;

        var exit = _mob.CurrentRoom.GetRandomExit();
        if (exit == null)
            return;

        // 50% failure chance (ROM-like)
        if (Random.Shared.Next(100) < 50)
        {
            _mob.Wait = 2;
            return;
        }

        _mob.CurrentTarget = null;
        _mob.Position = Position.Standing;

        world.Enqueue(new EnterRoomAction(_mob, exit.TargetRoom));
    }
}
