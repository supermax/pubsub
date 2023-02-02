using System;
using System.Runtime.CompilerServices;

namespace SuperMaxim.Core.Extensions
{
    public static class ValidationExtensions
    {
        public static void ThrowIfNull<T>(this T value, string memberName, [CallerMemberName] string callerName = "") where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(memberName, $"{memberName} cannot be null ({callerName})");
            }
        }

        public static void ThrowIfDefault<T>(this T value, string memberName, [CallerMemberName] string callerName = "")
        {
            if (Equals(value, default(T)))
            {
                throw new ArgumentOutOfRangeException(memberName, $"{memberName} has invalid value ({callerName})");
            }
        }

        public static void ThrowIf<T>(this T value, Predicate<T> predicate, string memberName, [CallerMemberName] string callerName = "") where T : class
        {
            if (predicate(value))
            {
                throw new ArgumentNullException(memberName, $"{memberName} has invalid value ({callerName})");
            }
        }

        public static void ThrowIfNullOrEmpty(this string value, string memberName, [CallerMemberName] string callerName = "")
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentNullException(memberName, $"{memberName} cannot be null ({callerName})");
            }
        }

        public static void ThrowIfNullOrEmpty(this Guid value, string memberName, [CallerMemberName] string callerName = "")
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentNullException(memberName, $"{memberName} cannot be null ({callerName})");
            }
        }

        public static void ThrowIfNullOrEmpty(this Guid? value, string memberName, [CallerMemberName] string callerName = "")
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentNullException(memberName, $"{memberName} cannot be null ({callerName})");
            }
        }
    }
}
