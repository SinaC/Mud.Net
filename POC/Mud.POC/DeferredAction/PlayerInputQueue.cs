using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class PlayerInputQueue
{
    private readonly Queue<(Mob player, string input)> _queue = new();

    public void Enqueue(Mob player, string input)
        => _queue.Enqueue((player, input));

    public void Process(World world)
    {
        while (_queue.Count > 0)
        {
            var (player, input) = _queue.Dequeue();
            var action = CommandParser.Parse(input, player, world);
            if (action != null)
            {
                world.Enqueue(action);
            }
        }
    }
}
