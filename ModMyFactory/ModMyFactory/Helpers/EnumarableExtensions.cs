using System;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactory.Helpers
{
    static class EnumarableExtensions
    {
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            if (comparer == null) comparer = Comparer<TKey>.Default;

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return default(TSource);

                TSource maxElement = enumerator.Current;
                TKey maxKey = selector.Invoke(maxElement);

                while (enumerator.MoveNext())
                {
                    TSource element = enumerator.Current;
                    TKey key = selector.Invoke(element);

                    if (comparer.Compare(key, maxKey) > 0)
                    {
                        maxElement = element;
                        maxKey = key;
                    }
                }

                return maxElement;
            }
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            if (comparer == null) comparer = Comparer<TKey>.Default;

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return default(TSource);

                TSource maxElement = enumerator.Current;
                TKey maxKey = selector.Invoke(maxElement);

                while (enumerator.MoveNext())
                {
                    TSource element = enumerator.Current;
                    TKey key = selector.Invoke(element);

                    if (comparer.Compare(key, maxKey) < 0)
                    {
                        maxElement = element;
                        maxKey = key;
                    }
                }

                return maxElement;
            }
        }

        public static bool Contains<TSource, TValue>(this IEnumerable<TSource> source, TValue value, Func<TSource, TValue> selector, IComparer<TValue> comparer = null)
        {
            if (comparer == null) comparer = Comparer<TValue>.Default;

            foreach (TSource sourceItem in source)
            {
                TValue selectedItem = selector(sourceItem);
                if (comparer.Compare(selectedItem, value) == 0) return true;
            }
            return false;
        }

        public static bool Contains<TSource, TValue>(this IEnumerable<TSource> source, TValue value, Func<TSource, TValue, bool> comparison)
        {
            foreach (TSource item in source)
            {
                if (comparison(item, value)) return true;
            }
            return false;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
        {
            foreach (var sourceItem in source)
                yield return sourceItem;

            yield return item;
        }

        public static IEnumerable<T> ConcatAll<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(collection => collection);
        }

        public static IEnumerable<T> EnumerateSingle<T>(this T value)
        {
            yield return value;
        }

        public static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool ifEmpty)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return ifEmpty;

                do
                {
                    T element = enumerator.Current;
                    if (predicate(element)) return true;
                } while (enumerator.MoveNext());

                return false;
            }
        }

        public static bool All<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool ifEmpty)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return ifEmpty;

                do
                {
                    T element = enumerator.Current;
                    if (!predicate(element)) return false;
                } while (enumerator.MoveNext());

                return true;
            }
        }
    }
}
