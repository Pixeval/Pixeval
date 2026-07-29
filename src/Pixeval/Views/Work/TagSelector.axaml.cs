// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.HighPerformance;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Pixeval.Views.Work;

public partial class TagSelector : UserControl
{
    public event EventHandler<TagSelector, (bool isPrivate, IReadOnlyList<string> tags)>? TagsSelected;

    public SimpleWorkType WorkType { get; init; }

    public long WorkId { get; init; }

    public static readonly DirectProperty<TagSelector, AvaloniaList<BookmarkTag>> TagsSourceProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, AvaloniaList<BookmarkTag>>(nameof(TagsSource), t => t.TagsSource,
            (t, v) => t.TagsSource = v);

    public AvaloniaList<BookmarkTag> TagsSource
    {
        get;
        set => SetAndRaise(TagsSourceProperty, ref field, value);
    } = [];

    public AvaloniaList<BookmarkTag> SelectedTags { get; } = new()
    {
        Validate = t =>
        {
            if (t is AddNewBookmarkTag)
                throw new ArgumentException();
        }
    };

    public static readonly DirectProperty<TagSelector, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, bool>(nameof(IsLoading), t => t.IsLoading, (t, v) => t.IsLoading = v);

    public bool IsLoading
    {
        get;
        set => SetAndRaise(IsLoadingProperty, ref field, value);
    }

    public static readonly DirectProperty<TagSelector, bool> IsPrivateProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, bool>(nameof(IsPrivate), t => t.IsPrivate, (t, v) => t.IsPrivate = v);

    public bool IsPrivate
    {
        get;
        set => SetAndRaise(IsPrivateProperty, ref field, value);
    }

    public TagSelector()
    {
        InitializeComponent();
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        TagsSelected?.Invoke(this, (IsPrivate, [.. SelectedTags.Select(t => t.Name)]));
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // 最多10个，递归删除
        if (sender is ListBox { SelectedItems: { Count: > 10 } items })
            items.RemoveAt(0);
    }

    public async Task ResetSourceAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        SelectedTags.Clear();
        TagsSource = [];

        try
        {
            if (WorkId is 0)
            {
                var publicTagsTask = GetTagsAsync(PrivacyPolicy.Public);
                var privateTagsTask = GetTagsAsync(PrivacyPolicy.Private);
                await Task.WhenAll(publicTagsTask, privateTagsTask);
                var tagSource = publicTagsTask.Result;
                tagSource.AddRange(privateTagsTask.Result);
                tagSource.Add(GetAddTag());
                TagsSource = tagSource;
            }
            else
            {
                var bookmarkDetail = await App.AppViewModel.MakoClient.GetWorkBookmarkDetailAsync(WorkType, WorkId);
                IsPrivate = bookmarkDetail.Restrict is PrivacyPolicy.Private;
                var tags = bookmarkDetail.Tags.Select(BookmarkDetailBookmarkTag.Create).ToArray();
                TagsSource = [.. tags, GetAddTag()];
                SelectedTags.AddRange(tags.Where(t => t.IsRegistered));
            }
        }
        finally
        {
            IsLoading = false;
        }

        return;

        async Task<AvaloniaList<BookmarkTag>> GetTagsAsync(PrivacyPolicy policy) => [.. await App.AppViewModel.MakoClient.WorkBookmarkTags(WorkType, PixevalSettings.MyId, policy).ToListAsync()];
    }

    private AddNewBookmarkTag GetAddTag()
    {
        return new AddNewBookmarkTag
        {
            Name = "",
            Count = 0,
            TagAdded = (_, tagName) => AddTag(tagName)
        };

        void AddTag(string name)
        {
            var tagsSource = TagsSource;
            var selectedTags = SelectedTags;
            if (tagsSource.Any(t => t.Name == name))
                return;

            var newTag = new BookmarkTag
            {
                Name = name,
                Count = 0
            };
            tagsSource.Insert(tagsSource.Count - 1, newTag);
            selectedTags.Add(newTag);
        }
    }
}
