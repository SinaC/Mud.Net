using System.Collections.Generic;

namespace Mud.DataStructures.Flags
{
    public interface IFlagValues<T>
    {
        IEnumerable<T> AvailableValues { get; }
        bool this[T flag] { get; } // return true if flag is in AvailableValues, false otherwise
    }
}
