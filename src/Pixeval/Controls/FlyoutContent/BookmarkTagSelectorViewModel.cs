// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls.FlyoutContent;

public partial class BookmarkTagSelectorViewModel : ObservableObject
{
    public ObservableCollection<string> SelectedTags { get; } = [];

    [ObservableProperty] public partial ObservableCollection<BookmarkTag> TokenViewSource { get; set; } = null!;
}
