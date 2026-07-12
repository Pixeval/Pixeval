// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.ViewModels;

public interface IRefCloneable<out T>
{
    T CloneRef();
}
