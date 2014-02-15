using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public static class ObservableCollectionExtensions
    {
        public static void Swap<T>(this ObservableCollection<T> collection, int index1, int index2)
        {
            Contract.Requires(index1 < collection.Count);
            Contract.Requires(index2 < collection.Count);
            if (index1 == index2)
                return;
            int min = Math.Min(index1, index2);
            int max = Math.Max(index1, index2);
            collection.Move(max, min);
            collection.Move(min + 1, max);
        }

        static int Partition<T>(ObservableCollection<T> collection, Func<T, long> valueOf, int left, int right, int pivotIndex)
        {
            Contract.Requires(left < right);
            Contract.Requires(right < collection.Count);
            Contract.Requires(pivotIndex >= left && pivotIndex <= right);
            long pivotValue = valueOf(collection[pivotIndex]);
            collection.Swap(pivotIndex, right);
            int stored = left;
            for (int i = left; i < right; i++)
            {
                if (valueOf(collection[i]) <= pivotValue)
                {
                    collection.Swap(i, stored);
                    stored = stored + 1;
                }
            }
            collection.Swap(stored, right);
            return stored;
        }

        static Random random = new Random();

        static void Quicksort<T>(ObservableCollection<T> collection, Func<T, long> valueOf, int left, int right)
        {
            Contract.Requires(left < right);
            Contract.Requires(right < collection.Count);
            if (left < right)
            {
                if ((right - left) < 10)
                    collection.Bubblesort(valueOf, left, right);
                else
                {
                    int pivotIndex = random.Next(left + 1, right);
                    int nextPivotIndex = Partition(collection, valueOf, left, right, pivotIndex);
                    Quicksort(collection, valueOf, left, nextPivotIndex - 1);
                    Quicksort(collection, valueOf, nextPivotIndex + 1, right);
                }
            }
        }

        public static void Quicksort<T>(this ObservableCollection<T> collection, Func<T, long> valueOf)
        {
            if (collection.Count <= 1)
                return;

            Quicksort(collection, valueOf, 0, collection.Count - 1);
        }

        public static void Bubblesort<T>(this ObservableCollection<T> collection, Func<T, long> valueOf, int left = 0, int right = -1)
        {
            if (right == -1)
                right = collection.Count - 1;

            bool isSorted = false;
            int firstIndex = left;
            while (!isSorted)
            {
                bool hasChangedLastSort = false;
                long min = valueOf(collection[firstIndex]);
                int minIndex = firstIndex;
                for (int i = right; i >= firstIndex; i--)
                {
                    long valueAtI = valueOf(collection[i]);
                    if (valueAtI < min)
                    {
                        min = valueAtI;
                        minIndex = i;
                    }
                }
                if (minIndex > firstIndex)
                {
                    collection.Swap(minIndex, firstIndex);
                    firstIndex++;
                    hasChangedLastSort = true;
                }
                if (!hasChangedLastSort)
                    isSorted = true;
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> ie)
        {
            foreach (var e in ie)
                collection.Add(e);
        }
    }
}
