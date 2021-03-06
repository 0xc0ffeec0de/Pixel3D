﻿using System;
using System.Diagnostics;

namespace RCRU.Engine
{
    // NOTE: This does not have a custom network serializer (should not serialize the internal array, just valid data)
    //       BUT: It probably shouldn't appear in the serialization anyway...


    /// <summary>Minimum-first priority queue, implemented with a heap, with 16-bit priorities</summary>
    public class ShortPriorityQueue<T>
    {
        public ShortPriorityQueue() : this(16) { }

        public ShortPriorityQueue(int capacity)
        {
            nodes = new Node[capacity];
        }


        public void Clear()
        {
            Array.Clear(nodes, 0, count);
            count = 0;
        }

        /// <summary>Do not use this method if T is or contains reference types.</summary>
        public void ClearFast()
        {
            count = 0;
        }


        #region Node Storage

        // TODO: Seperate arrays for priorities and values?
        struct Node
        {
            public ushort priority;
            public T value;
        }

        Node[] nodes;
        int count;

        public int Count { get { return count; } }
        public int Capacity { get { return nodes.Length; } }


        // This is a (min-first) priority queue.
        // Represented as a heap.
        // Represented as a binary-tree, where each child has higher priority number than its parent, filled from the left.
        // Stored in an array with the following indexing:
        //
        // childIndex[0] = 2*parentIndex + 1;
        // childIndex[1] = 2*parentIndex + 2;
        // parentIndex = (childIndex-1)/2; // <- relies on integer division rounding down

        #endregion


        public void Enqueue(T value, ushort priority)
        {
            if(count == nodes.Length)
                Array.Resize(ref nodes, nodes.Length * 2);

            // Insert
            int childIndex = count++;
            nodes[childIndex] = new Node { priority = priority, value = value };

            while(childIndex > 0) // While we are not the root node
            {
                int parentIndex = (childIndex - 1) / 2;
                
                // Keep swapping upwards until we reach the top
                if(nodes[childIndex].priority >= nodes[parentIndex].priority)
                    break;

                Node temp = nodes[childIndex];
                nodes[childIndex] = nodes[parentIndex];
                nodes[parentIndex] = temp;

                childIndex = parentIndex;
            }
        }


        public T Dequeue()
        {
            Debug.Assert(count > 0);

            // Take out the first value, and replace its node with the last
            T output = nodes[0].value;
            nodes[0] = nodes[--count];

            // Sort the value downwards to restore the heap property:
            int parentIndex = 0;
            while(true)
            {
                int childIndex = parentIndex*2 + 1;
                if(childIndex >= count)
                    break; // No children

                // Selct the smaller of the left/right children:
                if(childIndex+1 < count && nodes[childIndex+1].priority < nodes[childIndex].priority)
                    childIndex++;

                // Keep swapping downwards until parent is smaller than both children
                if(nodes[parentIndex].priority <= nodes[childIndex].priority)
                    break;

                Node temp = nodes[childIndex];
                nodes[childIndex] = nodes[parentIndex];
                nodes[parentIndex] = temp;

                parentIndex = childIndex;
            }

            return output;
        }


    }
}
