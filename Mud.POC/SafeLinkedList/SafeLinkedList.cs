using System.Collections;
using System.Diagnostics;

namespace Mud.POC.SafeLinkedList;

public class SafeLinkedList<T> : ICollection<T>
{
    // This SafeLinkedList is a doubly-Linked circular list.
    // Allowing adding/removing nodes during enumeration without invalidating the enumerator.
    internal SafeLinkedListNode<T>? head;
    internal int count;

    public SafeLinkedList()
    {
    }

    public SafeLinkedList(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        foreach (T item in collection)
        {
            AddLast(item);
        }
    }

    public int Count => count;

    public bool IsReadOnly => false;

    public void Add(T value)
    {
        AddLast(value);
    }

    public void Clear()
    {
        OnClearingList?.Invoke();

        var current = head;
        while (current != null)
        {
            var temp = current;
            current = current.Next;
            temp.Invalidate();
        }

        head = null;
        count = 0;
    }

    public bool Contains(T value)
    {
        return Find(value) != null;
    }

    public void CopyTo(T[] array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (index > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "index is bigger than collection length");
        }

        if (array.Length - index < Count)
        {
            throw new ArgumentException("Insufficient space");
        }

        var node = head;
        if (node != null)
        {
            do
            {
                array[index++] = node!.item;
                node = node.next;
            } while (node != head);
        }
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
        Count == 0
        ? Enumerable.Empty<T>().GetEnumerator()
        : GetEnumerator();

    public bool Remove(T value)
    {
        var node = Find(value);
        if (node != null)
        {
            InternalRemoveNode(node);
            return true;
        }
        return false;
    }

    //
    private SafeLinkedListNode<T>? Find(T value)
    {
        var node = head;
        var c = EqualityComparer<T>.Default;
        if (node != null)
        {
            if (value != null)
            {
                do
                {
                    if (c.Equals(node!.item, value))
                    {
                        return node;
                    }
                    node = node.next;
                } while (node != head);
            }
            else
            {
                do
                {
                    if (node!.item == null)
                    {
                        return node;
                    }
                    node = node.next;
                } while (node != head);
            }
        }
        return null;
    }

    private SafeLinkedListNode<T> AddLast(T value)
    {
        var result = new SafeLinkedListNode<T>(this, value);
        if (head == null)
        {
            InternalInsertNodeToEmptyList(result);
        }
        else
        {
            InternalInsertNodeBefore(head, result);
        }
        return result;
    }

    private void InternalInsertNodeBefore(SafeLinkedListNode<T> node, SafeLinkedListNode<T> newNode)
    {
        newNode.next = node;
        newNode.prev = node.prev;
        node.prev!.next = newNode;
        node.prev = newNode;
        count++;

        OnNodeAdded?.Invoke(newNode);
    }

    private void InternalInsertNodeToEmptyList(SafeLinkedListNode<T> newNode)
    {
        Debug.Assert(head == null && count == 0, "SafeLinkedList must be empty when this method is called!");

        newNode.next = newNode;
        newNode.prev = newNode;
        head = newNode;
        count++;

        OnNodeAdded?.Invoke(newNode);
    }

    internal void InternalRemoveNode(SafeLinkedListNode<T> node)
    {
        Debug.Assert(node.list == this, "Deleting the node from another list!");
        Debug.Assert(head != null, "This method shouldn't be called on empty list!");

        OnRemovingNode?.Invoke(node);

        if (node.next == node)
        {
            Debug.Assert(count == 1 && head == node, "this should only be true for a list with only one node");
            head = null;
        }
        else
        {
            node.next!.prev = node.prev;
            node.prev!.next = node.next;
            if (head == node)
            {
                head = node.next;
            }
        }
        node.Invalidate();
        count--;
    }

    internal static void ValidateNewNode(SafeLinkedListNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.list != null)
        {
            throw new InvalidOperationException("SafeLinkedListNodeIsAttached");
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

    internal delegate void NodeAddedHandler(SafeLinkedListNode<T> node);
    internal delegate void RemovingNodeHandler(SafeLinkedListNode<T> node);
    internal delegate void ClearingListHandler();

    internal event NodeAddedHandler? OnNodeAdded;
    internal event RemovingNodeHandler? OnRemovingNode;
    internal event ClearingListHandler? OnClearingList;

    public class Enumerator : IEnumerator<T>, IEnumerator
    {
        private readonly SafeLinkedList<T> _list;
        private SafeLinkedListNode<T>? _node;
        private T? _current;
        private int _index;

        internal Enumerator(SafeLinkedList<T> list)
        {
            _list = list;
            _node = list.head;
            _current = default;
            _index = 0;

            // register to list change events to make enumeration fail-safe
            list.OnNodeAdded += NodeAddedHandler;
            list.OnRemovingNode += RemovingNodeHandler;
            list.OnClearingList += ClearingListHandler;
        }

        public T Current => _current!;

        object? IEnumerator.Current
        {
            get
            {
                if (_index == 0 || (_index == _list.Count + 1))
                {
                    throw new InvalidOperationException("EnumOpCantHappen");
                }

                return Current;
            }
        }

        public bool MoveNext()
        {
            if (_node == null)
            {
                _index = _list.Count + 1;
                return false;
            }

            ++_index;
            _current = _node.item;
            _node = _node.next;
            if (_node == _list.head)
            {
                _node = null;
            }
            return true;
        }

        void IEnumerator.Reset()
        {
            _current = default;
            _node = _list.head;
            _index = 0;
        }

        private void NodeAddedHandler(SafeLinkedListNode<T> node)
        {
            if (_node == null) // enumeration reached the end, but a new node was added, continue enumeration
            {
                _node = node;
            }
        }

        private void RemovingNodeHandler(SafeLinkedListNode<T> node)
        {
            if (_node == node) // removing current node, change _node to next
            {
                _node = _node.next;
                if (_node == _list.head)
                    _node = null;
                if (_node != null)
                    _current = _node.item;
            }
        }

        private void ClearingListHandler()
        {
            _node = null;
            _index = _list.Count + 1;
        }

        public void Dispose()
        {
            if (_list != null)
            {
                _list.OnNodeAdded -= NodeAddedHandler;
                _list.OnRemovingNode -= RemovingNodeHandler;
                _list.OnClearingList -= ClearingListHandler;
            }
        }
    }
}

internal sealed class SafeLinkedListNode<T>
{
    internal SafeLinkedList<T>? list;
    internal SafeLinkedListNode<T>? next;
    internal SafeLinkedListNode<T>? prev;
    internal T item;

    public SafeLinkedListNode(T value)
    {
        item = value;
    }

    internal SafeLinkedListNode(SafeLinkedList<T> list, T value)
    {
        this.list = list;
        item = value;
    }

    public SafeLinkedList<T>? List => list;

    public SafeLinkedListNode<T>? Next => next == null || next == list!.head ? null : next;

    public SafeLinkedListNode<T>? Previous => prev == null || this == list!.head ? null : prev;

    public T Value
    {
        get => item;
        set => item = value;
    }

    internal void Invalidate()
    {
        list = null;
        next = null;
        prev = null;
    }
}
