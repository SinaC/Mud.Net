using System.Collections.Generic;
using System.Threading;

namespace Mud.DataStructures
{
    public sealed class ModifiableList<T> : List<T>
    {
        private readonly List<T> _pendingAdditions = new List<T>();
        private readonly List<T> _pendingDeletions = new List<T>();

        private int _activeEnumeratorsCount;

        public new void Add(T t)
        {
            if (_activeEnumeratorsCount == 0)
                base.Add(t);
            else
                _pendingAdditions.Add(t);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            if (_activeEnumeratorsCount == 0)
                base.AddRange(collection);
            else
                _pendingAdditions.AddRange(collection);
        }

        public new void Remove(T t)
        {
            if (_activeEnumeratorsCount == 0)
                base.Remove(t);
            else
                _pendingDeletions.Add(t);
        }

        public new IEnumerator<T> GetEnumerator()
        {
            Interlocked.Increment(ref _activeEnumeratorsCount);

            foreach (T t in this)
                yield return t;

            if (Interlocked.Decrement(ref _activeEnumeratorsCount) == 0)
            {
                // Add pending additions
                AddRange(_pendingAdditions);
                _pendingAdditions.Clear();
                // Remove pending deletions
                foreach (T item in _pendingDeletions)
                    base.Remove(item);
                _pendingDeletions.Clear();
            }
        }

    }
}
