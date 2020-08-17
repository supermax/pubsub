using System;
using System.Collections.Generic;

namespace SuperMaxim.Core.Extensions
{
    /// <summary>
    /// Collection Extensions
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Convert array to array of objects
        /// </summary>
        /// <param name="source">The source array</param>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <returns>The array of objects</returns>
        public static object[] ToObjectArray<T>(this T[] source)
        {
            if(source.IsNullOrEmpty())
            {
                return null;
            }
            var copy = new object[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }

        /// <summary>
        /// Check if collection is null or empty 
        /// </summary>
        /// <param name="source">The source collection</param>
        /// <typeparam name="T">The item type</typeparam>
        /// <returns>"true" in case array us null or empty</returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            if(source == null)
            {
                return true;
            }
            if(source.Count < 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Iterate thru the collection and execute the given action
        /// </summary>
        /// <param name="collection">The source collection</param>
        /// <param name="action">The action to execute during the iteration</param>
        /// <typeparam name="T">The type of the item</typeparam>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        /// <summary>
        /// Checks if string is null or empty
        /// </summary>
        /// <param name="source">The source string</param>
        /// <returns>"True" if string is null or empty</returns>
        public static bool IsNullOrEmpty(this string source)
        {
            var isEmpty = string.IsNullOrEmpty(source);
            return isEmpty;
        }
    }
}
