using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HCL_ODA_TestPAD.Functional.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> MatchingWith(this IEnumerable<string> myList, string itemToMatch)
        {
            foreach (var item in myList.Where(item => item == itemToMatch))
                yield return item;
        }
        public static IEnumerable<string> NotMatchingWith(this IEnumerable<string> myList, string itemToMatch)
        {
            foreach (var item in myList.Where(item => item != itemToMatch))
                yield return item;
        }
        public static IEnumerable<string> IgnoreNullOrEmptyOrSpace(this IEnumerable<string> myList)
        {
            foreach (var item in myList.Where(item => !string.IsNullOrEmpty(item) && item != " "))
                yield return item;
        }
        public static IEnumerable<string> MakeAllUpper(this IEnumerable<string> myList)
        {
            foreach (var item in myList)
                yield return item.ToUpper();
        }
        public static IEnumerable<string> MakeAllLower(this IEnumerable<string> myList)
        {
            foreach (var item in myList)
                yield return item.ToLower();
        }
        public static IEnumerable<T> MakeAllDefault<T>(this IEnumerable<T> myList)
        {
            foreach (var item in myList)
                yield return default;
        }
        public static IEnumerable<string> MatchingWithPattern(this IEnumerable<string> myList, string pattern)
        {
            foreach (var item in myList.Where(item => Regex.IsMatch(item, pattern)))
                yield return item;
        }
        public static IEnumerable<string> LengthEquals(this IEnumerable<string> myList, int itemLength)
        {
            foreach (var item in myList.Where(item => item.Length == itemLength))
                yield return item;
        }
        public static IEnumerable<string> LengthInRange(this IEnumerable<string> myList, int startOfRange, int endOfRange)
        {
            foreach (var item in myList.Where(item => item.Length >= startOfRange && item.Length <= endOfRange))
                yield return item;
        }
    }
}
