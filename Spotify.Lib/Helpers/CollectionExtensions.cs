using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        //public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this
        //    IEnumerable<TSource> source, int size)
        //{
        //    TSource[]? bucket = null;
        //    var count = 0;

        //    foreach (var item in source)
        //    {
        //        bucket ??= new TSource[size];

        //        bucket[count++] = item;
        //        if (count != size)
        //            continue;

        //        yield return bucket;

        //        bucket = null;
        //        count = 0;
        //    }

        //    if (bucket != null && count > 0)
        //        yield return bucket.Take(count).ToArray();
        //}

        //public static void Sort<TSource, TKey>(
        //    this ObservableCollection<TSource> source,
        //    Func<TSource, TKey> keySelector,
        //    bool ascending)
        //{
        //    var sortedSource = ascending ? source.OrderBy(keySelector).ToList() :
        //        source.OrderByDescending(keySelector).ToList();

        //    for (var i = 0; i < sortedSource.Count; i++)
        //    {
        //        var itemToSort = sortedSource[i];

        //        // If the item is already at the right position, leave it and continue.
        //        if (source.IndexOf(itemToSort) == i)
        //        {
        //            continue;
        //        }

        //        source.Remove(itemToSort);
        //        source.Insert(i, itemToSort);
        //    }
        //}
        public static IList<T> Sort<T, TKey>(this IEnumerable<T> col,
            Func<T, TKey> sortFunc,
            bool ascending)
        {
            return ascending ? new List<T>(col.OrderBy(sortFunc)) : new List<T>(col.OrderByDescending(sortFunc));
        }

        public static void Swap<T>(this IList<T> list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public static bool AddOrUpdate<T1, T2>(this IDictionary<T1, T2> dict, T1 key, T2 value)
        {
            if (dict.ContainsKey(key))
            {
                if (Equals(dict[key], value))
                    return false;
                dict[key] = value;
                return true;
            }

            dict.Add(key, value);
            return true;
        }

        public static void AddUnique<T>(this ICollection<T> destination, T item)
        {
            if (destination.Contains(item))
                return;
            destination.Add(item);
        }

        public static void AddUnique<T>(this ICollection<T> destination, params T[] source)
        {
            foreach (var obj in source)
                if (!destination.Contains(obj))
                    destination.Add(obj);
        }

        public static void AddUnique<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (var obj in source)
                if (!destination.Contains(obj))
                    destination.Add(obj);
        }

        public static void Add<T>(this ICollection<T> destination, params T[] source)
        {
            foreach (var obj in source)
                destination.Add(obj);
        }

        public static ICollection<T> AddReturn<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (var obj in source)
                destination.Add(obj);
            return destination;
        }


        public static bool Swap<T>(this IEnumerable<T> objectArray, int x, int y)
        {
            // check for out of range
            var enumerable = objectArray as T[] ?? objectArray.ToArray();
            if (enumerable.Length <= y || enumerable.Length <= x) return false;


            // swap index x and y
            var buffer = enumerable[x];
            enumerable[x] = enumerable[y];
            enumerable[y] = buffer;


            return true;
        }
    }
}