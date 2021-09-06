using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pixeval.Util
{
    public static class Objects
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this object? obj)
        {
            return obj is null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Negate(this bool b)
        {
            return !b;
        }

        public static async Task<string> HashAsync<T>(this T algorithm, byte[] bytes) where T : HashAlgorithm
        {
            await using var memoryStream = new MemoryStream(bytes);
            return (await algorithm.ComputeHashAsync(memoryStream)).ToHexString();
        }

        public static string ToHexString(this byte[] bytes)
        {
            return bytes.Select(b => b.ToString("X2")).Aggregate((s1, s2) => s1 + s2);
        }

        public static T CastOrThrow<T>(this object? obj)
        {
            // Debugger compliant: NullReferenceException will cause debugger to break, meanwhile the NRE is not supposed to be thrown by developer
            return (T) (obj ?? throw new InvalidCastException());
        }

        public static string Format(this string str, params object?[] args)
        {
            return string.Format(str, args);
        }

        /// <summary>
        /// Start inclusive, end inclusive
        /// </summary>
        public static bool InRange(this double i, (double, double) range)
        {
            var (startInclusive, endInclusive) = range;
            return i >= startInclusive && i <= endInclusive;
        }

        public static double CoerceIn(double i, (double, double) range)
        {
            var (startInclusive, endInclusive) = range;
            return Math.Max(startInclusive, Math.Min(i, endInclusive));
        }

        public static IEnumerable<TEnum> GetEnumValues<TEnum>(this Type type)
        {
            return type.GetEnumValues().Cast<TEnum>();
        }

        public static bool Inverse(ref this bool b) => b = !b;
    }
}