using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HCL_ODA_TestPAD.Functional.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T item in collection) action(item);
        }

        public static string ToCommaSeparatedString<T>(this IEnumerable<T> collection)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int index = 0;
            foreach (T item in collection)
            {
                if (index > 0)
                {
                    if (index < collection.Count() - 1) stringBuilder.Append(", ");
                    else if (index == collection.Count() - 1) stringBuilder.Append(" and ");
                }
                stringBuilder.Append(item.ToString());
                index++;
            }
            return stringBuilder.ToString();
        }


        public static string FillWithEnumMembers<T>(this ICollection<T> collection)
        {
            if (typeof(T).BaseType != typeof(Enum))
            {
                //throw new ArgumentException("The FillWithMembers<T> method can only be called with an enum as the generic type.");
                return "The FillWithMembers<T> method can only be called with an enum as the generic type.";
            }
            collection.Clear();
            foreach (string name in Enum.GetNames(typeof(T)))
            {
                collection.Add((T)Enum.Parse(typeof(T), name));
            }

            return string.Empty;
        }

        public static string GetDescription(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
            {
                return Enum.GetName(value.GetType(), value);
            }
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return Enum.GetName(value.GetType(), value);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> inputCollection)
        {
            return new ObservableCollection<T>(inputCollection);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> keys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (keys.Add(keySelector(element))) yield return element;
            }
        }

        /// <summary>
        /// Let’s use a simple example:
        /// We have a collection of 6 hats with different colors,
        /// where 2 of them are blue, 2 greens and 2 reds.
        /// We just want a collection of hats with the different possibilities of colors.
        /// With the DistinctBy the result will be a collection of 3 items: 1 blue, 1 green and 1 red hat.
        /// </summary>
        public static IEnumerable<T> DistinctByProperty<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
        //var uniqueCollection = collectionWithDuplicates.DistinctBy(x => x.MyProperty).ToList();

        //static IEnumerable<T> Where<T>(this IEnumerable<T> data, Func<T, bool> predicate)
        //{
        //    foreach (T value in data)
        //    {
        //        if (predicate(value)) yield return value;
        //    }
        //}

        //foreach (var coding in codings)
        //{
        //    if (!CodingDict.ContainsKey(coding))
        //    {
        //        CodingDict.Add(coding, new List<string>(){coding.Id
        //        });
        //    }
        //    CodingDict[coding].Add(coding.Id);
        //}

        public static IEnumerable<TResult> Flatten<T, TResult>(
            this IEnumerable<T> sequence, Func<T, Option<TResult>> map) =>
            sequence.Select(map)
                .OfType<Some<TResult>>()
                .Select(x => (TResult)x);

        public static Dictionary<K, V> Filter<K, V>(this Dictionary<K, V> dict, Predicate<KeyValuePair<K, V>> pred)
        {
            return dict.Where(it => pred(it)).ToDictionary(it => it.Key, it => it.Value);
        }
    }
}
