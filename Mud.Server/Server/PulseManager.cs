using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.Interfaces;
using System.Diagnostics;

namespace Mud.Server.Server;

[Export(typeof(IPulseManager)), Shared]
public class PulseManager : IPulseManager
{
    private class PulseEntry
    {
        public required string Name { get; init; }
        public required int PulseResetValue { get; init; }
        public required Action<int> PulseAction { get; init; }

        public required int PulseCurrentValue { get; set; }
    }

    private ILogger<PulseManager> Logger { get; }

    private readonly List<PulseEntry> _entries;

    public PulseManager(ILogger<PulseManager> logger)
    {
        Logger = logger;

        _entries = [];
    }

    public IEnumerable<string> PulseNames => _entries.Select(x => x.Name);

    public void Add(string name, int initialValue, int resetValue, Action<int> action)
    {
        _entries.Add(new PulseEntry
        {
            Name = name,
            PulseCurrentValue = initialValue,
            PulseResetValue = resetValue,
            PulseAction = action
        });
        Logger.LogInformation("Adding Pulse {name} initial value: {initialValue} reset value: {resetValue} method: {methodName}", name, initialValue, resetValue, action.Method.Name);
    }

    public void Pulse()
    {
        Stopwatch sw = new();
        foreach (var entry in _entries)
        {
            if (entry.PulseCurrentValue > 0)
                entry.PulseCurrentValue--;
            else
            {
                entry.PulseCurrentValue = entry.PulseResetValue;
                sw.Restart();
                entry.PulseAction(entry.PulseResetValue);
                sw.Stop();
                if (sw.ElapsedMilliseconds > Common.Pulse.PulseDelay)
                    Logger.LogWarning("PULSE SLOW: {name} in {duration} ms", entry.Name, sw.ElapsedMilliseconds);
                else
                    Logger.LogTrace("PULSE: {name} in {duration} ms", entry.Name, sw.ElapsedMilliseconds);
            }
        }
    }

    public void Pulse(string name)
    {
        var entry = _entries.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name));
        if (entry == null)
            return;
        entry.PulseAction(entry.PulseResetValue);
    }

    public void Clear()
    {
        Logger.LogInformation("Clear all pulses");
        _entries.Clear();
    }
}
