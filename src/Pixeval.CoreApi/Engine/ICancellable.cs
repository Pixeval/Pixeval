// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

namespace Pixeval.CoreApi.Engine;

public interface ICancellable
{
    bool IsCancelled { get; set; }
}
