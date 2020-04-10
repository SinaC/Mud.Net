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

using System.Collections.Generic;

namespace Mud.DataStructures.Graph
{
    /// <summary>
    /// Adjacency node
    /// Some operations are implemented using a Dictionary. In order for them to run fast T should have a good hash
    /// </summary>
    public class AdjacencyNode<T>
    {
        private readonly Dictionary<T, AdjacencyNode<T>> _adjacentNodes = new Dictionary<T, AdjacencyNode<T>>();

        public AdjacencyNode(T value)
        {
            Value = value;
        }

        public T Value
        {
            get; }

        public int EdgeCount => _adjacentNodes.Count;

        public IEnumerable<AdjacencyNode<T>> AdjacentNodes => _adjacentNodes.Values;

        /// <summary>
        /// Operation: O(1)
        /// </summary>
        public void AddEdgeTo(AdjacencyNode<T> node)
        {
            if (!HasEdgeTo(node))
            {
                _adjacentNodes.Add(node.Value, node);
            }
        }

        /// <summary>
        /// Operation: O(1)
        /// </summary>
        public void RemoveEdgeTo(AdjacencyNode<T> node)
        {
            _adjacentNodes.Remove(node.Value);
        }

        /// <summary>
        /// Operation: O(1)
        /// </summary>
        public bool HasEdgeTo(AdjacencyNode<T> node)
        {
            return _adjacentNodes.ContainsKey(node.Value);
        }

        public override bool Equals(object obj)
        {
            AdjacencyNode<T> f = obj as AdjacencyNode<T>;
            if (f == null)
            {
                return false;
            }

            return Equals(f);
        }

        public bool Equals(AdjacencyNode<T> f)
        {
            if (f == null)
            {
                return false;
            }

            return Value.Equals(f.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}