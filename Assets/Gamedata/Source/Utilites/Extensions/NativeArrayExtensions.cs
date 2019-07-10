using System;
using Unity.Collections;

namespace DoTs.Utilites
{
    public static class NativeArrayExtensions
    {
        public static void QuickSort<T>(this NativeArray<T> arr) where T : struct, IComparable<T> 
        {
            QuickSort(arr, 0, arr.Length - 1);
        }
        
        private static void QuickSort<T>(this NativeArray<T> arr, int startIndex, int endIndex) where T : struct, IComparable<T> 
        {
            if (startIndex >= endIndex)
            {
                return;
            }

            var pivot = Partition(arr, startIndex, endIndex);
            if (pivot > 1)
            {
                QuickSort(arr, startIndex, pivot - 1);
            }

            if (pivot + 1 < endIndex)
            {
                QuickSort(arr, pivot + 1, endIndex);
            }
        }

        private static int Partition<T>(NativeArray<T> arr, int startIndex, int endIndex) where T : struct, IComparable<T>
        {
            var pivot = arr[startIndex];
            while (true)
            {
                while (arr[startIndex].CompareTo(pivot) < 0)
                {
                    startIndex++;
                }

                while (arr[endIndex].CompareTo(pivot) > 0)
                {
                    endIndex--;
                }

                if (startIndex < endIndex)
                {
                    if (arr[startIndex].CompareTo(arr[endIndex]) == 0)
                    {
                        return endIndex;
                    }

                    T temp = arr[startIndex];
                    arr[startIndex] = arr[endIndex];
                    arr[endIndex] = temp;
                }
                else
                {
                    return endIndex;
                }
            }
        }
    }
}