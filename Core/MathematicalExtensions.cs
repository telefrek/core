namespace Telefrek.Core
{
    /// <summary>
    /// Some useful tools
    /// </summary>
    public static class MathematicalExtensions
    {
        #region GCD
        /// <summary>
        /// Implements the greatest common denominator between two integers
        /// </summary>
        /// <param name="x">The first value</param>
        /// <param name="y">The second value</param>
        /// <returns>The greatest common denominator between the values</returns>
        public static int GCD(this int x, int y)
        {
            var shift = 0;
            if (x == 0) return y;
            if (y == 0) return x;

            for (shift = 0; ((x | y) & 0x1) == 0x0; ++shift)
            {
                x >>= 1;
                y >>= 1;
            }

            while ((x & 0x1) == 0x0)
                x >>= 1;

            do
            {
                while ((y & 0x1) == 0x0)
                    y >>= 1;

                if (x > y)
                {
                    x ^= y;
                    y ^= x;
                    x ^= y;
                }

                y -= x;
            } while (y != 0);

            return x << shift;
        }

        /// <summary>
        /// Implements the greatest common denominator between two unsigned integers
        /// </summary>
        /// <param name="x">The first value</param>
        /// <param name="y">The second value</param>
        /// <returns>The greatest common denominator between the values</returns>
        public static uint GCD(this uint x, uint y)
        {
            var shift = 0;
            if (x == 0) return y;
            if (y == 0) return x;

            for (shift = 0; ((x | y) & 0x1) == 0x0; ++shift)
            {
                x >>= 1;
                y >>= 1;
            }

            while ((x & 0x1) == 0x0)
                x >>= 1;

            do
            {
                while ((y & 0x1) == 0x0)
                    y >>= 1;

                if (x > y)
                {
                    x ^= y;
                    y ^= x;
                    x ^= y;
                }

                y -= x;
            } while (y != 0);

            return x << shift;
        }


        /// <summary>
        /// Implements the greatest common denominator between 64 bit values
        /// </summary>
        /// <param name="x">The first value</param>
        /// <param name="y">The second value</param>
        /// <returns>The greatest common denominator between the values</returns>
        public static long GCD(this long x, long y)
        {
            var shift = 0;
            if (x == 0) return y;
            if (y == 0) return x;

            for (shift = 0; ((x | y) & 0x1) == 0x0; ++shift)
            {
                x >>= 1;
                y >>= 1;
            }

            while ((x & 0x1) == 0x0)
                x >>= 1;

            do
            {
                while ((y & 0x1) == 0x0)
                    y >>= 1;

                if (x > y)
                {
                    x ^= y;
                    y ^= x;
                    x ^= y;
                }

                y -= x;
            } while (y != 0);

            return x << shift;
        }

        /// <summary>
        /// Implements the greatest common denominator between unsigned 64 bit values
        /// </summary>
        /// <param name="x">The first value</param>
        /// <param name="y">The second value</param>
        /// <returns>The greatest common denominator between the values</returns>
        public static ulong GCD(this ulong x, ulong y)
        {
            var shift = 0;
            if (x == 0) return y;
            if (y == 0) return x;

            for (shift = 0; ((x | y) & 0x1) == 0x0; ++shift)
            {
                x >>= 1;
                y >>= 1;
            }

            while ((x & 0x1) == 0x0)
                x >>= 1;

            do
            {
                while ((y & 0x1) == 0x0)
                    y >>= 1;

                if (x > y)
                {
                    x ^= y;
                    y ^= x;
                    x ^= y;
                }

                y -= x;
            } while (y != 0);

            return x << shift;
        }
        #endregion

        #region MSB/LSB (Most/Least Significant Bit/Log Base 2)

        static int[] MSBDeBruijnLookup = new int[]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

        static int[] LSBDeBruijnLookup = new int[]
        {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };

        /// <summary>
        /// Uses a DeBruijn Lookup to calculate the MSB.
        /// </summary>
        /// <param name="x">The value to calculate the MSB for.</param>
        /// <returns>The position of the highest set bit.</returns>
        public static int MSB(this int x)
        {
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;

            return MSBDeBruijnLookup[(uint)(x * 0x07C4ACDDU) >> 27];
        }

        /// <summary>
        /// Uses a DeBruijn Lookup to calculate the LSB.
        /// </summary>
        /// <param name="x">The value to calculate the LSB for.</param>
        /// <returns>The position of the lowest set bit.</returns>
        public static int LSB(this int x) => LSBDeBruijnLookup[(uint)((x & -x) * 0x077CB531U) >> 27];
        #endregion
    }
}