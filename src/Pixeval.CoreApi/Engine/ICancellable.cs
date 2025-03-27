// Copyright (c) Mako.
// Licensed under the GPL v3 License.

namespace Mako.Engine;

public interface ICancellable
{
    bool IsCancelled { get; set; }
}
