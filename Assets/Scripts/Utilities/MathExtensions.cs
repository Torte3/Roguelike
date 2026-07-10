#nullable enable

namespace Utilities
{
    public static class MathExtensions
    {
        private static int Mod(this int value, int modulus)
        {
            if (modulus <= 0) throw new System.ArgumentOutOfRangeException(nameof(modulus), "modulus must be > 0");
            var remainder = value % modulus;
            return remainder < 0 ? remainder + modulus : remainder;
        }

        public static int WrapIndex(this int index, int count)
        {
            if (count <= 0) throw new System.ArgumentOutOfRangeException(nameof(count), "count must be > 0");
            return index.Mod(count);
        }
    }
}


