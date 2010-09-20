using System;
using System.Collections.Generic;
using System.Linq;

namespace RomTools
{
    public class ReadOnlyArray<T>
    {
        private readonly T[] array;

        public T this[int i]
        {
            get { return array[i]; }
        }

        public int Length
        {
            get { return array.Length; }
        }


        public ReadOnlyArray(IEnumerable<T> array)
        {
            this.array = array.ToArray();
        }

        public static explicit operator T[](ReadOnlyArray<T> roa)
        {
            var n = new T[roa.array.Length];
            Array.Copy(roa.array, n, roa.array.Length);

            return n;
        }
    }
}
