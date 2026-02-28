using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class MudEngine
{
    private readonly World _world = new();
    private readonly PlayerInputQueue _inputQueue = new();
    private bool _running = true;

    public void EnqueuePlayerCommand(Mob player, string input)
        => _inputQueue.Enqueue(player, input);

    public void Run()
    {
        while (_running)
        {
            // 1️ Process player inputs
            _inputQueue.Process(_world);

            // 2️ Tick world (combat, status effects, actions)
            _world.Tick();

            // 3️ Optional: render / notify players
            RenderWorld();

            // 4️ Simple fixed tick rate
            Thread.Sleep(1000); // 1 tick per second
        }
    }

    private void RenderWorld()
    {
        foreach (var room in _world.Rooms)
        {
            Console.WriteLine($"Room: {room.Name}");
            foreach (var mob in room.Mobs)
            {
                Console.WriteLine($" - {mob.Name} HP:{mob.HitPoints}/{mob.MaxHitPoints} Mana:{mob.Mana}/{mob.MaxMana}");
            }
        }
        Console.WriteLine("--------------------------------------------------");
    }

    public void Stop() => _running = false;
}
