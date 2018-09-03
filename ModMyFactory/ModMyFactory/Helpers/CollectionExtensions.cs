using System;
using System.Collections.Generic;

namespace ModMyFactory.Helpers
{
    static class CollectionExtensions
    {
        public static void ForEach<T>(this ICollection<T> source, Action<T> body)
        {
            foreach (T item in source)
            {
                body.Invoke(item);
            }
        }
    }
}
