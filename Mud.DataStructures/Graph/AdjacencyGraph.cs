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
    /// Undirected, unweightened graph.
    /// Each node and edge can only be added once. 
    /// If it gets added a second time it does just get ignored.
    /// 
    /// Some operations are implemented using a Dictionary. In order for them to run fast T should have a good hash
    /// </summary>
    public class AdjacencyGraph<T>
    {
        private readonly Dictionary<T, AdjacencyNode<T>> _nodes = new Dictionary<T, AdjacencyNode<T>>();

        public int NodeCount => _nodes.Count;

        public IEnumerable<AdjacencyNode<T>> Nodes => _nodes.Values;

        /// <summary>
        /// If we add an edge it checks if the node as already there, otherwise it adds the node.
        /// 
        /// Operation: O(1)
        /// </summary>
        public void AddEdge(T node1, T node2)
        {
            var startNode = _nodes.ContainsKey(node1) ? _nodes[node1] : null;
            if (startNode == null)
            {
                startNode = new AdjacencyNode<T>(node1);
                AddNodeIfNotAlreadyContained(startNode);
            }

            var endNode = _nodes.ContainsKey(node2) ? _nodes[node2] : null;
            if (endNode == null)
            {
                endNode = new AdjacencyNode<T>(node2);
                AddNodeIfNotAlreadyContained(endNode);
            }

            startNode.AddEdgeTo(endNode);
            endNode.AddEdgeTo(startNode);
        }

        private void AddNodeIfNotAlreadyContained(AdjacencyNode<T> node)
        {
            if (!_nodes.ContainsKey(node.Value))
            {
                _nodes.Add(node.Value, node);
            }
        }

        /// <summary>
        /// Removes the edge from nodes that are already part of the graph
        /// If the very last edge of a node to another one is removed the node is removed as well
        /// 
        /// Operation: O(1)
        /// </summary>
        public bool RemoveEdge(T start, T end)
        {
            var startNode = _nodes.ContainsKey(start) ? _nodes[start] : null;
            var endNode = _nodes.ContainsKey(end) ? _nodes[end] : null;

            if (startNode == null || endNode == null)
            {
                return false;
            }

            startNode.RemoveEdgeTo(endNode);

            if (startNode.EdgeCount == 0)
            {
                _nodes.Remove(start);
            }

            endNode.RemoveEdgeTo(startNode);
            if (endNode.EdgeCount == 0)
            {
                _nodes.Remove(end);
            }

            return true;
        }

        /// <summary>
        /// If we remove a node the associated edges are removed as well
        /// 
        /// Operation: O(m) where m is the number of edges
        /// </summary>
        public bool RemoveNode(T node)
        {
            var nodeToRemove = _nodes.ContainsKey(node) ? _nodes[node] : null;
            if (nodeToRemove == null)
            {
                return false;
            }

            foreach (AdjacencyNode<T> n in nodeToRemove.AdjacentNodes)
            {
                n.RemoveEdgeTo(nodeToRemove);
            }

            _nodes.Remove(node);

            return true;
        }

        /// <summary>
        /// Operation: O(1)
        /// </summary>
        public bool ContainsNode(T node)
        {
            return _nodes.ContainsKey(node);
        }

        /// <summary>
        /// Operation: O(1)
        /// </summary>
        public bool AreDirectlyConnected(T node1, T node2)
        {
            var startNode = _nodes.ContainsKey(node1) ? _nodes[node1] : null;
            var endNode = _nodes.ContainsKey(node2) ? _nodes[node2] : null;

            if (startNode == null || endNode == null)
            {
                return false;
            }

            return startNode.HasEdgeTo(endNode);
        }
    }
}