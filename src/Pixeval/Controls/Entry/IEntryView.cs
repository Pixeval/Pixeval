// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;

namespace Pixeval.Controls;

public interface IEntryView<out TSortableEntryViewViewModel> : IScrollViewHost where TSortableEntryViewViewModel : ISortableEntryViewViewModel
{
    TSortableEntryViewViewModel ViewModel { get; }

    AdvancedItemsView AdvancedItemsView { get; }

    bool Focus(FocusState state);
}
