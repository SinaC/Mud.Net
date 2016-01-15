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

using System;
using System.Collections.Generic;
using Mud.DataStructures.HeapPriorityQueue;

namespace Mud.DataStructures.Graph
{
    /// <summary>
    /// Dijkstra
    /// Some operations are implemented using a Dictionary. In order for them to run fast T should have a good hash
    /// </summary>
    public class Dijkstra<T>
    {
        public AdjacencyNode<T> Source
        {
            get;
            private set;
        }

        private Dictionary<AdjacencyNode<T>, AdjacencyNode<T>> PreviousNode
        {
            get;
            set;
        }

        /// <summary>
        /// Runs the Dijkstra algorithm which calculates the shortest paths from the source to any other node
        /// which is reachable from there.
        /// 
        /// If this method is called multiple times with the same source it does only calculate the paths the first time
        /// 
        /// Exceptions:
        /// ArgumentException if the nodes Enumerable is null, the source is null or the source is not part of the graph (the nodes)
        /// </summary>
        public void Run(IEnumerable<AdjacencyNode<T>> nodes, AdjacencyNode<T> source)
        {
            if (nodes == null || source == null)
            {
                throw new ArgumentException("Nodes Enumerable or Source is null");
            }

            if (Source != null && Source.Equals(source))
            {
                return;
            }

            /**
             * Initialize the Algorithm
             */
            Source = source;

            // Holds the shortest distance between the source and the node
            Dictionary<AdjacencyNode<T>, int> distance = new Dictionary<AdjacencyNode<T>, int>();
            // Holds the node from which we need to go to the current node if we are taking the shortest path
            PreviousNode = new Dictionary<AdjacencyNode<T>, AdjacencyNode<T>>();
            // Fast Access to the Node (of the Nodes to inspect) which has the shortest distance and thus needs to be processed next
            // if we processed all nodes in that queue or the remaining ones are not reachable the algorithm is finished
            HeapPriorityQueue<AdjacencyNode<T>> distanceQueue = new HeapPriorityQueue<AdjacencyNode<T>>();

            foreach (AdjacencyNode<T> n in nodes)
            {
                // previous nodes are unknown at the start
                PreviousNode.Add(n, null);
                // distance is assumed to be the maximum possible value. Therefore it can be improved if we find a shorter one
                distance.Add(n, int.MaxValue);
                distanceQueue.Enqueue(n, int.MaxValue);
            }

            if (!distanceQueue.Contains(source))
            {
                throw new ArgumentException("The source is not part of the graph (nodes)");
            }

            /**
             * Execute the Algorithm
             */
            distance[Source] = 0;
            distanceQueue.UpdatePriority(Source, 0);

            while (!distanceQueue.IsEmpty())
            {
                // The nearest node is a node which has never been reached (otherwise its path would have been improved)
                // This means all other nodes can also not be reached and our algorithm is finished...
                if (distanceQueue.PeekPriority() == int.MaxValue)
                {
                    break;
                }

                AdjacencyNode<T> nearestNode = distanceQueue.Dequeue();

                // Check all neighbours that still need to be inspected
                foreach (AdjacencyNode<T> neighbour in nearestNode.AdjacentNodes)
                {
                    if (!distanceQueue.Contains(neighbour))
                    {
                        continue;
                    }

                    // calculate distance with the currently inspected neighbour
                    int neighbourDist = distance[nearestNode] + 1;

                    // set the neighbour as shortest if it is better than the currently known shortest distance
                    if (neighbourDist < distance[neighbour])
                    {
                        distance[neighbour] = neighbourDist;
                        distanceQueue.UpdatePriority(neighbour, neighbourDist);
                        PreviousNode[neighbour] = nearestNode;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the path from the source to the destination. It does include the destination as its last point. The first point
        /// is not the source but the first neighbour of it.
        /// 
        /// Operation executes in O(n)
        /// 
        /// Note: before we can get a path we need to run the algorithm
        /// </summary>
        public LinkedList<T> GetShortestPathTo(T destination)
        {
            if (Source == null)
            {
                throw new InvalidOperationException("You need to Run the algorithm first before calculating paths...");
            }

            AdjacencyNode<T> destinationNode = new AdjacencyNode<T>(destination);

            // building path going back from the destination to the source always taking the nearest node
            LinkedList<T> path = new LinkedList<T>();
            path.AddFirst(destinationNode.Value);

            while (PreviousNode[destinationNode] != null)
            {
                destinationNode = PreviousNode[destinationNode];
                path.AddFirst(destinationNode.Value);
            }

            path.RemoveFirst();

            return path;
        }
    }
}