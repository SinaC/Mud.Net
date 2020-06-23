using System.Collections.Generic;

namespace Mud.Server.Interfaces.GameAction
{
    public interface ICommandParameter
    {
        string RawValue { get; }
        bool IsAll { get; } // all.xxx
        int Count { get; }
        string Value { get; }

        List<string> Tokens { get; }

        bool IsNumber { get; }
        bool IsLong { get; }

        int AsNumber { get; }
        long AsLong { get; }
    }
}
