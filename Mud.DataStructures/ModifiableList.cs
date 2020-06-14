using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Mud.DataStructures
{
    // Define other methods and classes here
    public sealed class ModifiableList<T> : ICollection<T>, IEnumerable<T>
    {
        private class PendingAction
        {
            public enum PendingActionTypes
            {
                Add,
                Remove,
                Clear
            }

            public PendingActionTypes PendingActionType { get; set; }
            public T Item { get; set; }
        }

        private readonly Queue<PendingAction> _pendingActions = new Queue<PendingAction>(); // TODO: one level of pending action by iteration level

        private readonly List<T> _list = new List<T>();

        private int _activeEnumeratorsCount;

        public int Count 
        {
            get
            {
                if (Volatile.Read(ref _activeEnumeratorsCount) == 0)
                    return _list.Count;
                return _list.Count;
                //// Iterate thru pending actions and count
                //int count = _list.Count;
                //foreach (var pendingAction in _pendingActions)
                //{
                //    switch (pendingAction.PendingActionType)
                //    {
                //        case PendingAction.PendingActionTypes.Clear:
                //            count = 0;
                //            break;
                //        case PendingAction.PendingActionTypes.Add:
                //            count++;
                //            break;
                //        case PendingAction.PendingActionTypes.Remove:
                //            count--; // TODO: this is wrong, if we remove an element not found in list
                //            break;
                //    }
                //}
                //return count;
            }
        }

        public bool IsReadOnly => true;

        public void Add(T item)
        {
            if (Volatile.Read(ref _activeEnumeratorsCount) == 0)
            {
                _list.Add(item);
                return;
            }
            // Enqueue 'add' pending action
            _pendingActions.Enqueue(new PendingAction
            {
                PendingActionType = PendingAction.PendingActionTypes.Add,
                Item = item
            });
        }

        public void Clear()
        {
            if (Volatile.Read(ref _activeEnumeratorsCount) == 0)
            {
                _list.Clear();
                return;
            }
            // Enqueue 'clear' pending action
            _pendingActions.Enqueue(new PendingAction
            {
                PendingActionType = PendingAction.PendingActionTypes.Clear
            });
        }

        public bool Contains(T item)
        {
            if (Volatile.Read(ref _activeEnumeratorsCount) == 0)
                return _list.Contains(item);
            return _list.Contains(item);
            //return ContainsIncludingPendingActions(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (Volatile.Read(ref _activeEnumeratorsCount) == 0)
            {
                _list.CopyTo(array, arrayIndex);
                return;
            }
            _list.CopyTo(array, arrayIndex);
            //// 1) copy list
            //_list.CopyTo(array, arrayIndex);
            //// 2) Iterate thru pending actions and copy/remove/clear
            //int currentIndex = arrayIndex + _list.Count;
            //foreach (var pendingAction in _pendingActions)
            //{
            //    switch (pendingAction.PendingActionType)
            //    {
            //        case PendingAction.PendingActionTypes.Clear:
            //            for (int i = arrayIndex; i < currentIndex; i++)
            //                array[i] = default;
            //            break;
            //        case PendingAction.PendingActionTypes.Add:
            //            array[currentIndex] = pendingAction.Item;
            //            currentIndex++;
            //            break;
            //        case PendingAction.PendingActionTypes.Remove:
            //            currentIndex--;
            //            array[currentIndex] = default;
            //            break;
            //    }
            //}
        }

        public bool Remove(T item)
        {
            if (Volatile.Read(ref _activeEnumeratorsCount) == 0)
                return _list.Remove(item);
            bool found = _list.Contains(item);
            _pendingActions.Enqueue(new PendingAction
            {
                PendingActionType = PendingAction.PendingActionTypes.Remove,
                Item = item
            });
            return found;
            //// Check if found in list
            //bool found = ContainsIncludingPendingActions(item);
            //// Enqueue 'remove' pending action
            //_pendingActions.Enqueue(new PendingAction
            //{
            //    PendingActionType = PendingAction.PendingActionTypes.Remove,
            //    Item = item
            //});
            //return found;
        }

        public IEnumerator<T> GetEnumerator()
        {
            // TODO:
            Interlocked.Increment(ref _activeEnumeratorsCount);

            foreach (T t in _list)
                yield return t;

            if (Interlocked.Decrement(ref _activeEnumeratorsCount) == 0)
            {
                // perform pending action
                while (_pendingActions.Count > 0)
                {
                    PendingAction action = _pendingActions.Dequeue();
                    switch (action.PendingActionType)
                    {
                        case PendingAction.PendingActionTypes.Add:
                            _list.Add(action.Item);
                            break;
                        case PendingAction.PendingActionTypes.Remove:
                            _list.Remove(action.Item);
                            break;
                        case PendingAction.PendingActionTypes.Clear:
                            _list.Clear();
                            break;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool ContainsIncludingPendingActions(T item)
        {
            // 1) Check in actual list
            bool found = _list.Contains(item);
            // 2) Iterate thru pending actions
            //  clear -> not found
            //  add item -> found
            //  remove item -> not found
            foreach (var pendingAction in _pendingActions)
            {
                switch (pendingAction.PendingActionType)
                {
                    case PendingAction.PendingActionTypes.Clear:
                        found = false;
                        break;
                    case PendingAction.PendingActionTypes.Add:
                        if (item.Equals(pendingAction.Item))
                            found = true;
                        break;
                    case PendingAction.PendingActionTypes.Remove:
                        if (item.Equals(pendingAction.Item))
                            found = false;
                        break;
                }
            }
            return found;
        }
    }


    //    public sealed class ModifiableList<T> : List<T>
    //    {
    //        private readonly List<T> _pendingAdditions = new List<T>();
    //        private readonly List<T> _pendingDeletions = new List<T>();

    //        private int _activeEnumeratorsCount;

    //        public new void Add(T t)
    //        {
    //            if (_activeEnumeratorsCount == 0)
    //                base.Add(t);
    //            else
    //                _pendingAdditions.Add(t);
    //        }

    //        public new void AddRange(IEnumerable<T> collection)
    //        {
    //            if (_activeEnumeratorsCount == 0)
    //                base.AddRange(collection);
    //            else
    //                _pendingAdditions.AddRange(collection);
    //        }

    //        public new void Remove(T t)
    //        {
    //            if (_activeEnumeratorsCount == 0)
    //                base.Remove(t);
    //            else
    //                _pendingDeletions.Add(t);
    //        }

    //        public new IEnumerator<T> GetEnumerator()
    //        {
    //            Interlocked.Increment(ref _activeEnumeratorsCount);

    //            foreach (T t in this)
    //                yield return t;

    //            if (Interlocked.Decrement(ref _activeEnumeratorsCount) == 0)
    //            {
    //                // Add pending additions
    //                AddRange(_pendingAdditions);
    //                _pendingAdditions.Clear();
    //                // Remove pending deletions
    //                foreach (T item in _pendingDeletions)
    //                    base.Remove(item);
    //                _pendingDeletions.Clear();
    //            }
    //        }

    //    }
}
