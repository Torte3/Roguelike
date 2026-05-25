#nullable enable

namespace Utilities
{
    public static class MathExtensions
    {
        public static int Mod(this int value, int modulus)
        {
            if (modulus <= 0) throw new System.ArgumentOutOfRangeException(nameof(modulus), "modulus must be > 0");
            var remainder = value % modulus;
            return remainder < 0 ? remainder + modulus : remainder;
        }

        public static long Mod(this long value, long modulus)
        {
            if (modulus <= 0) throw new System.ArgumentOutOfRangeException(nameof(modulus), "modulus must be > 0");
            var remainder = value % modulus;
            return remainder < 0 ? remainder + modulus : remainder;
        }

        public static float Mod(this float value, float modulus)
        {
            if (modulus <= 0f) throw new System.ArgumentOutOfRangeException(nameof(modulus), "modulus must be > 0");
            var remainder = value % modulus;
            return remainder < 0 ? remainder + modulus : remainder;
        }

        public static double Mod(this double value, double modulus)
        {
            if (modulus <= 0d) throw new System.ArgumentOutOfRangeException(nameof(modulus), "modulus must be > 0");
            var remainder = value % modulus;
            return remainder < 0d ? remainder + modulus : remainder;
        }

        public static int WrapIndex(this int index, int count)
        {
            if (count <= 0) throw new System.ArgumentOutOfRangeException(nameof(count), "count must be > 0");
            return index.Mod(count);
        }
    }
}


