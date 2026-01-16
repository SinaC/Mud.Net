//
//  Copyright 2012  Patrick Uhlmann
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

namespace Mud.DataStructures.HeapPriorityQueue;

/// <summary>
/// Heap priority queue. Uses a Dictionary to speed up the UpdatePriority method. It is only allowed to add each element once
/// Some operations are implemented using a Dictionary. In order for them to run fast T should have a good hash
/// </summary>
public class HeapPriorityQueue<T>
    where T : notnull
{
    private static readonly int DEFAULTCAPACITY = 1024;

    private KeyValueEntry<int, T>[] _queue;

    private readonly Dictionary<T, int> _entryToPos;

    public HeapPriorityQueue()
        : this(DEFAULTCAPACITY)
    {
    }

    public HeapPriorityQueue(int capacity)
    {
        _queue = new KeyValueEntry<int, T>[capacity];
        _entryToPos = new Dictionary<T, int>(capacity);
    }

    public int Count { get; private set; }

    public bool IsEmpty()
    {
        return Count == 0;
    }

    /// <summary>
    /// Operation: O(1)
    /// </summary>
    public void Clear()
    {
        Count = 0;
        _queue = new KeyValueEntry<int, T>[DEFAULTCAPACITY];
        _entryToPos.Clear();
    }

    /// <summary>
    /// Operation: O(1)
    /// </summary>
    public bool Contains(T element)
    {
        return _entryToPos.ContainsKey(element);
    }

    /// <summary>
    /// Operation: O(1)
    /// </summary>
    public int PeekPriority()
    {
        if (IsEmpty())
        {
            return 0;
        }

        return _queue[0].Key;
    }

    /// <summary>
    /// Operation: O(1)
    /// </summary>
    public T? Peek()
    {
        if (IsEmpty())
        {
            return default;
        }

        return _queue[0].Value;
    }

    /// <summary>
    /// If the queue is empty we do just return null
    /// 
    /// Operation: O(log n)
    /// </summary>
    public T? Dequeue()
    {
        if (IsEmpty())
        {
            return default;
        }

        KeyValueEntry<int, T> result = _queue[0];

        _queue[0] = _queue[Count - 1];
        RemovePosition(result.Value);
        UpdatePosition(_queue[0].Value, 0);
        _queue[Count - 1] = default!;
        Count--;
        BubbleDown(0);

        return result.Value;
    }

    /// <summary>
    /// Operation: O(log n)
    /// 
    /// InvalidOperationException: If we add an element which is alread in the Queue
    /// </summary>
    public void Enqueue(T element, int priority)
    {
        if (Contains(element))
        {
            throw new InvalidOperationException("Cannot add an element which is already in the queue");
        }

        if (Count == _queue.Length)
        {
            KeyValueEntry<int, T>[] oldQueue = _queue;
            _queue = new KeyValueEntry<int, T>[oldQueue.Length * 3 / 2 + 1];
            Array.Copy(oldQueue, _queue, oldQueue.Length);
        }

        _queue[Count] = new KeyValueEntry<int, T>(priority, element);
        UpdatePosition(element, Count);
        Count++;

        BubbleUp(Count - 1);
    }

    /// <summary>
    /// Operation: O(log n)
    /// 
    /// InvalidOperationException: If we want to update the priority of an element which is not in the queue
    /// </summary>
    public void UpdatePriority(T element, int newPrio)
    {
        if (!_entryToPos.TryGetValue(element, out int pos))
        {
            throw new InvalidOperationException("Cannot update the priority of an element which is not in the queue");
        }

        int oldPrio = _queue[pos].Key;

        if (oldPrio == newPrio)
        {
            return;
        }

        _queue[pos].Key = newPrio;

        if (oldPrio < newPrio)
        {
            BubbleDown(pos);
        }
        else
        {
            BubbleUp(pos);
        }
    }

    /// <summary>
    /// Moving root to correct location
    /// </summary>
    private void BubbleDown(int i)
    {
        while (true)
        {
            var minPos = i;
            var min = _queue[i];
            var left = 2 * i + 1;
            var right = 2 * i + 2;

            if (left < Count && _queue[left].Key < min.Key)
            {
                minPos = left;
                min = _queue[left];
            }

            if (right < Count && _queue[right].Key < min.Key)
            {
                minPos = right;
                min = _queue[right];
            }

            if (min == _queue[i])
            {
                break;
            }
            else
            {
                // swap
                _queue[minPos] = _queue[i];
                _queue[i] = min;

                UpdatePosition(_queue[minPos].Value, minPos);
                UpdatePosition(_queue[i].Value, i);

                i = minPos;
            }
        }
    }

    /// <summary>
    /// Moving last element of last level to correct location
    /// </summary>
    private void BubbleUp(int i)
    {
        int n = i;
        int up = (n - 1) / 2;

        while (up >= 0 && _queue[up].Key > _queue[n].Key)
        {
            var entry = _queue[up];
            _queue[up] = _queue[n];
            _queue[n] = entry;

            UpdatePosition(_queue[n].Value, n);
            UpdatePosition(_queue[up].Value, up);

            n = up;
            up = (up - 1) / 2;
        }
    }

    private void UpdatePosition(T element, int pos)
    {
        _entryToPos.Remove(element);

        _entryToPos.Add(element, pos);
    }

    private void RemovePosition(T element)
    {
        _entryToPos.Remove(element);
    }
}