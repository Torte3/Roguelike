#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities
{
    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var item in ie)
            {
                action(item);
            }
        }

        public static Enumerator GetEnumerator(this Enum bits)
        {
            return new Enumerator(bits);
        }

        public struct Enumerator
        {
            private int bits;
            private int count;
            private int position;

            public Enumerator(Enum bits)
            {
                this.bits = Convert.ToInt32(bits);
                count = NumOfBits(this.bits);
                position = -1;
            }

            public bool MoveNext()
            {
                while (0 < count)
                {
                    ++position;
                    if ((bits & 1) != 0)
                    {
                        --count;
                        bits >>= 1;
                        return true;
                    }

                    bits >>= 1;
                }

                return false;
            }

            public int Current => 1 << position;
        }

        public static int NumOfBits(int bits)
        {
            bits = (bits & 0x55555555) + ((bits >> 1) & 0x55555555);
            bits = (bits & 0x33333333) + ((bits >> 2) & 0x33333333);
            bits = (bits & 0x0f0f0f0f) + ((bits >> 4) & 0x0f0f0f0f);
            bits = (bits & 0x00ff00ff) + ((bits >> 8) & 0x00ff00ff);
            return (bits & 0x0000ffff) + ((bits >> 16) & 0x0000ffff);
        }

        public static IEnumerable<(T item, int index)> Index<T>(this IEnumerable<T> ie)
        {
            return ie.Select((item, index) => (item, index));
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T value)
        {
            var i = 0;
            foreach (var item in source)
            {
                if (EqualityComparer<T>.Default.Equals(item, value))
                    return i;
                i++;
            }
            return -1;
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        {
            return source.Where(item => item != null).Cast<T>();
        }

        public static IEnumerable<Vector2Int> RectRange(this RectInt rect)
        {
            return RectRange(rect.xMin, rect.yMin, rect.width, rect.height);
        }

        public static IEnumerable<Vector2Int> RectRange(Vector2Int min, Vector2Int size)
        {
            return RectRange(min.x, min.y, size.x, size.y);
        }

        public static IEnumerable<Vector2Int> RectRange(int xMin, int yMin, int width, int height)
        {
            for (var x = xMin; x < xMin + width; x++)
            for (var y = yMin; y < yMin + height; y++)
                yield return new Vector2Int(x, y);
        }

        public static IEnumerable<Vector2Int> CircleRange(Vector2Int center, float radius)
        {
            for (var x = -Mathf.FloorToInt(radius); x <= Mathf.FloorToInt(radius); x++)
            for (var y = -Mathf.FloorToInt(radius); y <= Mathf.FloorToInt(radius); y++)
                if (x * x + y * y <= radius * radius)
                    yield return new Vector2Int(x, y) + center;
        }

        public static T MinBy<T, U>(this IEnumerable<T> xs, Func<T, U> key) where U : IComparable<U>
        {
            return xs.Aggregate((a, b) => key(a).CompareTo(key(b)) < 0 ? a : b);
        }

        public static T MaxBy<T, U>(this IEnumerable<T> xs, Func<T, U> key) where U : IComparable<U>
        {
            return xs.Aggregate((a, b) => key(a).CompareTo(key(b)) > 0 ? a : b);
        }

        public static T MinByOrDefault<T, U>(this IEnumerable<T> xs, Func<T, U> key, T defaultValue)
            where U : IComparable<U>
        {
            if (!xs.Any())
            {
                return defaultValue;
            }

            return xs.Aggregate((a, b) => key(a).CompareTo(key(b)) < 0 ? a : b);
        }

        public static T MaxByOrDefault<T, U>(this IEnumerable<T> xs, Func<T, U> key, T defaultValue)
            where U : IComparable<U>
        {
            if (!xs.Any())
            {
                return defaultValue;
            }

            return xs.Aggregate((a, b) => key(a).CompareTo(key(b)) > 0 ? a : b);
        }

        public static void SynchronizeWith<T>(this ICollection<T> collectionA, IEnumerable<T> collectionB)
        {
            // コレクションAの要素のうち、コレクションBに存在しないものを削除する
            var itemsToRemove = collectionA.Except(collectionB).ToList();
            foreach (var item in itemsToRemove)
            {
                collectionA.Remove(item);
            }

            // コレクションBの要素のうち、コレクションAに存在しないものを追加する
            var itemsToAdd = collectionB.Except(collectionA).ToList();
            foreach (var item in itemsToAdd)
            {
                collectionA.Add(item);
            }
        }

        public static List<T> TakeAndRemove<T>(this List<T> list, int count)
        {
            var result = list.Take(count).ToList();
            list.RemoveAll(item => result.Contains(item));
            return result;
        }

        public static T GetAtRandomAndRemove<T>(this List<T> list)
        {
            var result = list.GetAtRandom();
            list.Remove(result);
            return result;
        }

        public static List<T> GetAtRandomAndRemove<T>(this List<T> list, int n)
        {
            var result = list.GetAtRandom(n);
            list.RemoveAll(item => result.Contains(item));
            return result;
        }

        public static IEnumerable<T> CreateNewInstances<T>(int count) where T : new()
        {
            return Enumerable.Range(0, count).Select(_ => new T());
        }
    }
}