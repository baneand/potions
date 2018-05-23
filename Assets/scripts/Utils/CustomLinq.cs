using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CustomLinq
{
    public static class CustomLinq
    {
        public static TSource First<TSource>([NotNull] this IEnumerable<TSource> source, [NotNull] Func<TSource, bool> func)
        {
            foreach (TSource item in source)
            {
                if (item != null && func(item))
                {
                    return item;
                }
            }
            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>([NotNull] this IEnumerable<TSource> source)
        {
            foreach (TSource item in source)
            {
                if (item != null)
                {
                    return item;
                }
            }
            return default(TSource);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>([NotNull] this IEnumerable<TSource> source, [NotNull] Func<TSource, TResult> func)
        {
            var result = new List<TResult>();
            foreach (var sourceItem in source)
            {
                result.Add(func(sourceItem));
            }
            return result;
        }

        public static TSource[] ToArray<TSource>([NotNull] this IEnumerable<TSource> source)
        {
            return new List<TSource>(source).ToArray();
        }
    }
}