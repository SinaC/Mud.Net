using System.Collections.Generic;

namespace Mud.DataStructures.Flags
{
    public abstract class FlagValuesBase<T> : IFlagValues<T>
    {
        protected abstract HashSet<T> HashSet { get; }

        public bool this[T flag] => HashSet.Contains(flag);

        public IEnumerable<T> AvailableValues => HashSet;
    }
}
