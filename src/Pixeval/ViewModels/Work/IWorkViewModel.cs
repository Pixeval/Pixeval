// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Misaki;
using Pixeval.Filters.Nodes;
using Pixeval.Models.Filters;

namespace Pixeval.ViewModels;

public interface IWorkViewModel : INotifyPropertyChanged
{
    bool IsBookmarkSupported { get; }

    IArtworkInfo Entry { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.AddToBookmarkCommand"/>
    IAsyncRelayCommand<(IReadOnlyList<string>? Tags, bool IsPrivate, Control? Control)> AddToBookmarkCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.BookmarkCommand"/>
    IAsyncRelayCommand<Control?> BookmarkCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveCommand"/>
    IAsyncRelayCommand<Control?> SaveCommand { get; }

    bool Filter(FilterNode node) => WorkFilterEvaluator.Filter(Entry, node);
}
