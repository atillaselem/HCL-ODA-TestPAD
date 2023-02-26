using System;
using System.Collections.Generic;

namespace HCL_ODA_TestPAD.Functional.Extensions
{
    public static class DumpExtensions
    {
        public static void Dump<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (var item in @this)
            {
                action(item);
            }
        }
        public static void Dump<TItem>(this IEnumerable<TItem> @this, Action<int, TItem> action)
        {
            foreach (var (item, index) in @this.WithIndex())
            {
                action(index, item);
            }
        }
    }
}
