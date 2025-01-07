// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

namespace Pixeval.Utilities;

public interface IDeepCloneable<out TSelf> where TSelf : IDeepCloneable<TSelf>
{
    public TSelf DeepClone();
}
