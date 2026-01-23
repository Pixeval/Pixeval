// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

namespace Pixeval.Controls;

public interface IFactory<in T, out TSelf>
{
    static abstract TSelf CreateInstance(T entry);
}
