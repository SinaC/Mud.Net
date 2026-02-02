namespace Mud.Server.CommandParser.Interfaces;

public interface ICommandParameter
{
    string RawValue { get; }
    bool IsAll { get; } // all.xxx or all
    bool IsAllOnly { get; } // all
    int Count { get; }
    string Value { get; }

    List<string> Tokens { get; }

    bool IsNumber { get; }
    bool IsLong { get; }

    int AsNumber { get; }
    long AsLong { get; }
}
