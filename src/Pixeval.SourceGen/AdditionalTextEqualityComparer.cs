using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Pixeval.SourceGen
{
    public class AdditionalTextEqualityComparer : IEqualityComparer<AdditionalText>
    {
        public bool Equals(AdditionalText x, AdditionalText y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Path == y.Path;
        }

        public int GetHashCode(AdditionalText obj)
        {
            return obj.Path.GetHashCode();
        }
    }
}