using Mud.Server.CommandParser.Interfaces;

namespace Mud.Server.CommandParser;

public class CommandParameter : ICommandParameter
{
    public static readonly CommandParameter InvalidCommandParameter = new(null!);

    public string RawValue { get; }
    public bool IsAll { get; } // all.xxx or all
    public bool IsAllOnly { get; } // all
    public int Count { get; }
    public string Value { get; } = default!; // all.xxx -> will contain xxx   if all -> will contain all

    public List<string> Tokens { get; }

    public bool IsNumber => int.TryParse(Value, out _);
    public bool IsLong => long.TryParse(Value, out _);

    public int AsNumber
    {
        get
        {
            if (int.TryParse(Value, out var intValue))
                return intValue;
            return 0;
        }
    }

    public long AsLong
    {
        get
        {
            if (long.TryParse(Value, out var longValue))
                return longValue;
            return 0;
        }
    }

    private CommandParameter(string rawValue)
    {
        Tokens = [];

        RawValue = rawValue;
    }

    public CommandParameter(string rawValue, string value, int count)
        : this(rawValue)
    {
        Value = value;
        Count = count;
        Tokens = value.Split(' ').ToList();
    }

    public CommandParameter(string rawValue, string value, bool isAll, bool isAllOnly)
        : this(rawValue)
    {
        Value = value;
        IsAll = isAll;
        IsAllOnly = isAllOnly;
        Tokens = value.Split(' ').ToList();
    }

    public override string ToString() => RawValue;
}
