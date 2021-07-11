using System.Runtime.CompilerServices;

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
    }
}