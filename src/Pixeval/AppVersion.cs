using System;

namespace Pixeval
{
    public enum IterationStage
    {
        Alpha,
        Preview,
        ReleaseCandidate,
        Stable
    }

    // A slightly modified semantic version
    public record AppVersion(IterationStage Stage, int Major, int Minor, int Patch, int? PreReleaseSpecifier = null) : IComparable<AppVersion>
    {
        public int CompareTo(AppVersion? other)
        {
            if (other is { } version)
            {
                if (version.Stage.CompareTo(Stage) is var i and not 0)
                {
                    return i;
                }  

                if (version.Major.CompareTo(Major) is var j and not 0)
                {
                    return j;
                }

                if (version.Minor.CompareTo(Minor) is var k and not 0)
                {
                    return k;
                }

                if (version.Patch.CompareTo(Patch) is var n and not 0)
                {
                    return n;
                }

                if (version.PreReleaseSpecifier is { } thatSpecifier && PreReleaseSpecifier is { } specifier && thatSpecifier.CompareTo(specifier) is var m and not 0)
                {
                    return m;
                }

                return 0;
            }

            return 1;
        }

        public override string ToString()
        {
            var versionNumber = $"v{Major}.{Minor}.{Patch}";
            var str = Stage switch
            {
                IterationStage.Alpha => $"{versionNumber}-alpha",
                IterationStage.Preview => $"{versionNumber}-preview",
                IterationStage.ReleaseCandidate => $"{versionNumber}-rc",
                IterationStage.Stable => $"{versionNumber}",
                _ => throw new ArgumentOutOfRangeException()
            };
            return Stage is not IterationStage.Stable && PreReleaseSpecifier is { } specifier ? $"{str}.{specifier}" : str;
        }
    }
}