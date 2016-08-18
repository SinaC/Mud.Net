using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mud.Logger;

namespace Mud.Server.Server
{
    public class PulseManager
    {
        private class PulseEntry
        {
            public int PulseCurrentValue { get; set; }
            public int PulseResetValue { get; set; }
            public Action PulseAction { get; set; }
        }

        private readonly List<PulseEntry> _entries;

        public PulseManager()
        {
            _entries = new List<PulseEntry>();
        }

        public void Add(int initialValue, int resetValue, Action method)
        {
            _entries.Add(new PulseEntry
            {
                PulseCurrentValue = initialValue,
                PulseResetValue = resetValue,
                PulseAction = method
            });
        }

        public void Pulse()
        {
            Stopwatch sw = new Stopwatch();
            foreach (PulseEntry entry in _entries)
            {
                if (entry.PulseCurrentValue > 0)
                    entry.PulseCurrentValue--;
                else
                {
                    entry.PulseCurrentValue = entry.PulseResetValue;
                    sw.Restart();
                    entry.PulseAction();
                    sw.Stop();
                    Log.Default.WriteLine(LogLevels.Trace, $"PULSE: {entry.PulseAction.Method.Name} in {sw.ElapsedMilliseconds} ms");
                }
            }
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }
}
