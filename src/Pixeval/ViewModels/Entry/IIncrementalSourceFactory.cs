// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Mako.Engine;

namespace Pixeval.ViewModels;

public interface IIncrementalSourceFactory<in T, out TSelf>
{
    static abstract TSelf CreateInstance(IFetchEngine<T> fetchEngine, int limit = -1);
}
