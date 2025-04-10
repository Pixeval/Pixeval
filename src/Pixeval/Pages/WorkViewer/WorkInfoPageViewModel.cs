// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.Mvvm.ComponentModel;
using Misaki;

namespace Pixeval.Pages;

public partial class WorkInfoPageViewModel(IArtworkInfo entry) : ObservableObject
{
    public IArtworkInfo Entry { get; } = entry;
}
