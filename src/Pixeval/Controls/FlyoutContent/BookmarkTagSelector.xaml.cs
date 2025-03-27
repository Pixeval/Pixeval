// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Util;
using WinUI3Utilities;

namespace Pixeval.Controls.FlyoutContent;

public sealed partial class BookmarkTagSelector : UserControl
{
    [GeneratedDependencyProperty(DefaultValue = SimpleWorkType.IllustAndManga)]
    public partial SimpleWorkType Type { get; set; }

    public bool IsPrivate
    {
        get;
        set
        {
            field = value;
            ResetSource();
        }
    }

    public BookmarkTagSelectorViewModel ViewModel { get; } = new();

    public IEnumerable<string> SelectedTags => ViewModel.SelectedTags;

    public BookmarkTagSelector()
    {
        InitializeComponent();
        ViewModel.SelectedTags.CollectionChanged += SelectedTags_CollectionChanged;
        ResetSource();
    }

    partial void OnTypePropertyChanged(DependencyPropertyChangedEventArgs e) => ResetSource();

    private void SelectedTags_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is { } newItems)
        {
            foreach (var newItem in newItems)
                if (ViewModel.TokenViewSource.Contains(newItem) && !TokenView.SelectedItems.Contains(newItem))
                    TokenView.SelectedItems.Add(newItem.To<BookmarkTag>());
        }
        else if (e.OldItems is { } oldItems)
            foreach (var oldItem in oldItems)
                if (TokenView.SelectedItems.IndexOf(oldItem) is var index and not -1)
                    TokenView.SelectedItems.RemoveAt(index);
    }

    private async void ResetSource()
    {
        _stopTracking = true;
        var policy = CheckBox.IsChecked is true ? PrivacyPolicy.Private : PrivacyPolicy.Public;
        ViewModel.TokenViewSource = [.. (await MakoHelper.GetBookmarkTagsAsync(policy, Type))[1..]];
        TokenView.SelectedItems.Clear();
        foreach (var item in ViewModel.TokenViewSource)
            if (ViewModel.SelectedTags.Contains(item.Name))
                TokenView.SelectedItems.Add(item);
        _stopTracking = false;
    }

    private void TokenView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_stopTracking)
            return;
        foreach (var newItem in e.AddedItems)
            if (!ViewModel.SelectedTags.Contains(newItem))
                ViewModel.SelectedTags.Add(newItem.To<BookmarkTag>().Name);
        foreach (var oldItem in e.RemovedItems)
            if (ViewModel.SelectedTags.IndexOf(oldItem.To<BookmarkTag>().Name) is var index and not -1)
                ViewModel.SelectedTags.RemoveAt(index);
    }

    private bool _stopTracking;

    private void TokenizingTextBox_OnTokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs e)
    {
        if (ViewModel.SelectedTags.Contains(e.TokenText))
            e.Cancel = true;
    }
}
