using System;
using System.Collections.Generic;

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
    }
}
