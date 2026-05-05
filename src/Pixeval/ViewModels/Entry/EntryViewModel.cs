// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics;
using Misaki;

namespace Pixeval.ViewModels;

[DebuggerDisplay("{Entry}")]
public abstract class EntryViewModel<T>(T entry) : ViewModelBase where T : IMisakiModel
{
    public T Entry { get; } = entry;

    public abstract Uri AppUri { get; }

    public abstract Uri WebsiteUri { get; }
}
