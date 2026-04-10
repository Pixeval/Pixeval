using System.Numerics;

namespace Pixeval.Utilities;

internal static class MathUtilities
{
    const double DoubleEpsilon = 2.2204460492503131e-016;

    public static bool GreaterThan<T>(T value1, T value2) where T : IFloatingPoint<T>
    {
        return (value1 > value2) && !AreClose(value1, value2);
    }

    public static bool AreClose<T>(T value1, T value2) where T : IFloatingPoint<T>
    {
        //in case they are Infinities (then epsilon check does not work)
        if (value1 == value2) return true;
        T eps = (T.Abs(value1) + T.Abs(value2) + T.CreateChecked(10.0)) * T.CreateChecked(DoubleEpsilon);
        T delta = value1 - value2;
        return (-eps < delta) && (eps > delta);
    }

    public static bool GreaterThanOrClose<T>(T value1, T value2) where T : IFloatingPoint<T>
    {
        return (value1 > value2) || AreClose(value1, value2);
    }
}
