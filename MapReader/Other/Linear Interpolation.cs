using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MapReader.Other
{
    internal class Interpolation
    {
        public static float LinearInterp(float x, float[] xp, float[] fp)
        {
            if (xp.Length != fp.Length)
                throw new ArgumentException("xp and fp must be the same length", nameof(xp));

            if (xp.Length == 0)
                throw new ArgumentException("kys bozo", nameof(xp));


            if (x <= xp[0])
                return fp[0];

            if (x >= xp[^1])
                return fp[^1];

            int index = BinarySearch(xp, x) - 1;

            float t = (x - xp[index]) / (xp[index + 1] - xp[index]);
            return fp[index] + (fp[index + 1] - fp[index]) * t;
        }

        public static float[] LinearInterp(float[] x, float[] xp, float[] fp)
        {
            float[] y = new float[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                y[i] = LinearInterp(x[i], xp, fp);
            }
            return y;
        }

        public static double Lerp(double start, double final, double amount)
        {
            return start + (final - start) * amount;
        }

        /// <summary> Returns the index of the first number in the array strictly greater than the key. </summary>
        private static int BinarySearch(float[] array, float key)
        {
            int min = 0;
            int max = array.Length - 1;
            while (min != max)
            {
                int mid = (min + max) / 2;
                if (key < array[mid])
                    max = mid;
                else
                    min = mid + 1;
            }
            return min;
        }
    }
}
