using System.Collections.Generic;

namespace Mud.DataStructures.Flags
{
    public interface IFlagValues<T>
    {
        IEnumerable<T> AvailableValues { get; }
        bool this[T flag] { get; } // return true if flag is in AvailableValues, false otherwise

        void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<T> values);
    }

    public enum UnknownFlagValueContext
    {
        IsSet,
        HasAny,
        HasAll,
        Set,
        UnSet,
    }
}
