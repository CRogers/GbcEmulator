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
    }
}
