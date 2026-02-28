using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class LookRoomAction : IGameAction
{
    private readonly Mob _viewer;

    public LookRoomAction(Mob viewer)
    {
        _viewer = viewer;
    }

    public void Execute(World world)
    {
        if (_viewer.IsDead) return;

        var room = _viewer.CurrentRoom;
        if (room == null) return;

        world.Enqueue(new ScriptAction(ctx =>
        {
            ctx.Notify($"== {room.Name} ==");

            // Mobs (excluding self)
            var otherMobs = room.Mobs
                .Where(m => m != _viewer && !m.IsDead)
                .ToList();

            if (otherMobs.Any())
                ctx.Notify("You see: " + string.Join(", ", otherMobs.Select(m => m.Name)));

            // Items
            if (room.Items.Any())
                ctx.Notify("Items here: " + string.Join(", ", room.Items.Select(i => i.Name)));

            // Exits
            if (room.Exits.Any())
                ctx.Notify("Exits: " + string.Join(", ", room.Exits.Select(e => e.TargetRoom.Name)));
        }));
    }
}
