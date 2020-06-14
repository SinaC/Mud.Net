using System.Collections;
using System.Collections.Generic;

namespace Mud.DataStructures
{
    public class SafeList<T> : IEnumerable<T>, ICollection<T>
    {
        private SafeListNode<T> _head;

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public void Add(T value) // add first
        {
            SafeListNode<T> node = new SafeListNode<T>
            {
                Value = value
            };
            if (_head != null)
            {
                node.Next = _head;
                _head.Previous = node;
            }
            _head = node;
            Count++;
        }

        public void Clear()
        {
            while (_head != null)
            {
                SafeListNode<T> next = _head.Next;
                _head.Previous = null;
                _head.Next = null;
                _head = next;
            }
            Count = 0;
        }

        public bool Remove(T value)
        {
            SafeListNode<T> current = _head;
            while (current != null)
            {
                if (current.Value.Equals(value))
                {
                    if (current.Next != null)
                        current.Next.Previous = current.Previous;
                    if (current.Previous != null)
                        current.Previous.Next = current.Next;
                    if (current == _head)
                        _head = _head.Next;
                    Count--;
                    return true;
                }
                current = current.Next;
            }
            return false;
        }

        public bool Contains(T item)
        {
            SafeListNode<T> current = _head;
            while (current != null)
            {
                if (current.Value.Equals(item))
                    return true;
                current = current.Next;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int index = arrayIndex;
            SafeListNode<T> current = _head;
            while (current != null)
            {
                array[index] = current.Value;
                index++;
                current = current.Next;
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            if (_head == null)
                yield break;
            SafeListNode<T> current = _head;
            SafeListNode<T> next = current.Next;
            while (true)
            {
                //current.Dump("ENUMERATOR");
                yield return current.Value;
                current = next;
                if (current == null)
                    yield break;
                next = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal class SafeListNode<TNode>
        {
            public SafeListNode<TNode> Previous { get; set; }
            public SafeListNode<TNode> Next { get; set; }
            public TNode Value { get; set; }
        }
    }
}
