using System;
using System.Collections.Generic;
using System.Linq;

namespace RomTools
{
    public static class ExtensionMethods
    {
        public static bool EqualsAny<T>(this T obj, params T[] values)
        {
            return values.Contains(obj);
        }

        public static byte SetBit(this byte b, int bit, bool on)
        {
            if (on)
                return (byte)(b | (1 << bit));
            else
                return (byte)(b & ~(1 << bit));
        }

        public static bool GetBit(this byte b, int bit)
        {
            int mask = 1 << bit;
            return (b & mask) == mask;
        }

        public static int SetBit(this int b, int bit, bool on)
        {
            if (on)
                return b | (1 << bit);
            else
                return b & ~(1 << bit);
        }

        public static bool GetBit(this int b, int bit)
        {
            int mask = 1 << bit;
            return (b & mask) == mask;
        }

        public static int Max(this int i, int a)
        {
            if (i > a)
                return a;

            return i;
        }

        public static T[] ExtendRight<T>(this T[] array, int times)
        {
            var ret = new T[array.Length * times];
            for (int i = 0; i < times; i++)
                Array.Copy(array, 0, ret, i * array.Length, array.Length);

            return ret;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToDictionary(p => p.Key, p => p.Value);
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<Tuple<TKey, TValue>> source)
        {
            return source.ToDictionary(p => p.Item1, p => p.Item2);
        }
    }
}
