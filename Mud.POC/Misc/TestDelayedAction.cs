using System;
using System.Diagnostics;

namespace Mud.POC.Misc
{
    // This would allow to add casting time to spell
    public class DelayedAction<T> // Looks similar to PeriodicAura
    {
        public T Actor { get; set; }
        public int TickDelay { get; set; } // delay between 2 ticks, each tick TickAction is called
        public Action<T, int> TickAction { get; set; }
        public int TotalTick { get; set; } // total number of ticks before calling ReleaseAction
        public Action<T> ReleaseAction { get; set; }
    }

    public class TestDelayedAction
    {
        public static void Test()
        {
            DelayedAction<string> spell = new DelayedAction<string>
            {
                Actor = "tsekwa",
                TickDelay = 4, // every second
                TickAction = (actor, tickCount) => Debug.WriteLine(actor + " is concentrating..."),
                TotalTick = 5,
                ReleaseAction = actor => Debug.WriteLine(actor + " releases a spell.")
            };
        }
    }
}
