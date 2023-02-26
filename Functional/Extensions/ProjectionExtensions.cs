using System;
using System.Collections.Generic;

namespace HCL_ODA_TestPAD.Functional.Extensions
{
    public static class ProjectionExtensions
    {
        public static TResult Map<T, TResult>(
            this T @this, Func<T, TResult> func)
            => func(@this);

        public static IEnumerable<TResult> Map<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TResult> mapper)
        {
            foreach (TSource element in source)
            {
                yield return mapper(element);
            }
        }


        public static T Tee<T>(this T @this, Action<T> action)
        {
            action(@this);
            return @this;
        }

        public static TResult IfNotNull<T, TResult>(this T r, Func<T, TResult> selector)
        {
            return r != null ? selector(r) : default;
        }
        public static T Branch<T>(this T @this, Predicate<T> selector)
        {
            return selector(@this) ? @this : default;
        }

        public static IEnumerable<TResult> GenerateEnumerable<T, TResult>(this T @this, TResult type)
        {
            return new List<TResult>();
        }
    }
}
