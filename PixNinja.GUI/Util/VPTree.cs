﻿using System;
using System.Collections.Generic;

namespace PixNinja.GUI.Util;

using DistType = Int32;

public delegate DistType CalculateDistance<in T>(T item1, T item2);

public sealed class VpTree<T>
{
    private T[] _items;
    private DistType _tau;
    private Node? _root;
    private Random _rand; // Used in BuildFromPoints
    private CalculateDistance<T> _calculateDistance;
    
    public VpTree(T[] items, CalculateDistance<T> distanceCalculator)
    {
        _rand = new Random(); // Used in BuildFromPoints
        Create(items, distanceCalculator);
    }

    public void Create(T[] newItems, CalculateDistance<T> distanceCalculator)
    {
        _items = newItems;
        _calculateDistance = distanceCalculator;
        _root = BuildFromPoints(0, newItems.Length);
    }

    public List<(T, DistType)> SearchByMaxDist(T target, int maxDist)
    {
        throw new NotImplementedException();
    }

    public List<(T, DistType)> Search(T target, int numberOfResults)
    {
        List<HeapItem> closestHits = new List<HeapItem>();

        // Reset tau to longest possible distance
        _tau = DistType.MaxValue;

        // Start search
        Search(_root, target, numberOfResults, closestHits);

        // Temp arrays for return values
        List<(T, DistType)> returnResults = new();

        // We have to reverse the order since we want the nearest object to be first in the array
        for (int i = numberOfResults - 1; i > -1; i--)
        {
            returnResults.Add((this._items[closestHits[i].Index], closestHits[i].Dist));
        }

        return returnResults;
    }



    private sealed class Node // This cannot be struct because Node referring to Node causes error CS0523
    {
        public int Index;
        public DistType Threshold;
        public Node? Left;
        public Node? Right;

        public Node()
        {
            this.Index = 0;
            this.Threshold = 0;
            this.Left = null;
            this.Right = null;
        }
    }

    private sealed class HeapItem
    {
        public readonly int Index;
        public readonly DistType Dist;

        public HeapItem(int index, DistType dist)
        {
            this.Index = index;
            this.Dist = dist;
        }

        public static bool operator <(HeapItem h1, HeapItem h2)
        {
            return h1.Dist < h2.Dist;
        }

        public static bool operator >(HeapItem h1, HeapItem h2)
        {
            return h1.Dist > h2.Dist;
        }
    }

    private Node? BuildFromPoints(int lowerIndex, int upperIndex)
    {
        if (upperIndex == lowerIndex)
        {
            return null;
        }

        var node = new Node();
        node.Index = lowerIndex;

        if (upperIndex - lowerIndex > 1)
        {
            Swap(_items, lowerIndex, this._rand.Next(lowerIndex + 1, upperIndex));

            int medianIndex = (upperIndex + lowerIndex) / 2;

            nth_element(_items, lowerIndex + 1, medianIndex, upperIndex - 1,
                (i1, i2) => Comparer<DistType>.Default.Compare(
                    _calculateDistance(_items[lowerIndex], i1), _calculateDistance(_items[lowerIndex], i2)));

            node.Threshold = this._calculateDistance(this._items[lowerIndex], this._items[medianIndex]);

            node.Left = BuildFromPoints(lowerIndex + 1, medianIndex);
            node.Right = BuildFromPoints(medianIndex, upperIndex);
        }

        return node;
    }

    private void Search(Node? node, T target, int numberOfResults, List<HeapItem> closestHits)
    {
        if (node == null)
        {
            return;
        }

        DistType dist = this._calculateDistance(this._items[node.Index], target);

        // We found entry with shorter distance
        if (dist < this._tau)
        {
            if (closestHits.Count == numberOfResults)
            {
                // Too many results, remove the first one which has the longest distance
                closestHits.RemoveAt(0);
            }

            // Add new hit
            closestHits.Add(new HeapItem(node.Index, dist));

            // Reorder if we have numberOfResults, and set new tau
            if (closestHits.Count == numberOfResults)
            {
                closestHits.Sort((a, b) => Comparer<DistType>.Default.Compare(b.Dist, a.Dist));
                this._tau = closestHits[0].Dist;
            }
        }

        if (node.Left == null && node.Right == null)
        {
            return;
        }

        if (dist < node.Threshold)
        {
            if (dist - this._tau <= node.Threshold)
            {
                this.Search(node.Left, target, numberOfResults, closestHits);
            }

            if (dist + this._tau >= node.Threshold)
            {
                this.Search(node.Right, target, numberOfResults, closestHits);
            }
        }
        else
        {
            if (dist + this._tau >= node.Threshold)
            {
                this.Search(node.Right, target, numberOfResults, closestHits);
            }

            if (dist - this._tau <= node.Threshold)
            {
                this.Search(node.Left, target, numberOfResults, closestHits);
            }
        }
    }

    private static void Swap(T[] arr, int index1, int index2)
    {
        (arr[index1], arr[index2]) = (arr[index2], arr[index1]);
    }

    private static void nth_element(T[] array, int startIndex, int nthToSeek, int endIndex, Comparison<T> comparison)
    {
        var from = startIndex;
        var to = endIndex;

        // if from == to we reached the kth element
        while (from < to)
        {
            int r = from, w = to;
            var mid = array[(r + w) / 2];

            // stop if the reader and writer meets
            while (r < w)
            {
                if (comparison(array[r], mid) > -1)
                {
                    // put the large values at the end
                    (array[w], array[r]) = (array[r], array[w]);
                    w--;
                }
                else
                {
                    // the value is smaller than the pivot, skip
                    r++;
                }
            }

            // if we stepped up (r++) we need to step one down
            if (comparison(array[r], mid) > 0)
            {
                r--;
            }

            // the r pointer is on the end of the first k elements
            if (nthToSeek <= r)
            {
                to = r;
            }
            else 
            {
                from = r + 1;
            }
        }
    }
}
