using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Pixeval.Views.Work;

public partial class TagSelector : UserControl
{
    public event EventHandler<TagSelector, (bool isPrivate, IReadOnlyList<string> tags)>? TagsSelected;

    public static readonly DirectProperty<TagSelector, SimpleWorkType> WorkTypeProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, SimpleWorkType>(nameof(WorkType), t => t.WorkType, (t, v) => t.WorkType = v);

    public SimpleWorkType WorkType
    {
        get;
        set => SetAndRaise(WorkTypeProperty, ref field, value);
    }

    public static readonly DirectProperty<TagSelector, AvaloniaList<BookmarkTag>> PublicTagsSourceProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, AvaloniaList<BookmarkTag>>(nameof(PublicTagsSource), t => t.PublicTagsSource,
            (t, v) => t.PublicTagsSource = v);

    public AvaloniaList<BookmarkTag> PublicTagsSource
    {
        get;
        set
        {
            var oldCurrentTagsSource = CurrentTagsSource;
            SetAndRaise(PublicTagsSourceProperty, ref field, value);
            if (!IsPrivate)
                RaisePropertyChanged(CurrentTagsSourceProperty, oldCurrentTagsSource, CurrentTagsSource);
        }
    } = [];

    public static readonly DirectProperty<TagSelector, AvaloniaList<BookmarkTag>> PrivateTagsSourceProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, AvaloniaList<BookmarkTag>>(nameof(PrivateTagsSource), t => t.PrivateTagsSource,
            (t, v) => t.PrivateTagsSource = v);

    public AvaloniaList<BookmarkTag> PrivateTagsSource
    {
        get;
        set
        {
            var oldCurrentTagsSource = CurrentTagsSource;
            SetAndRaise(PrivateTagsSourceProperty, ref field, value);
            if (IsPrivate)
                RaisePropertyChanged(CurrentTagsSourceProperty, oldCurrentTagsSource, CurrentTagsSource);
        }
    } = [];

    public static readonly DirectProperty<TagSelector, AvaloniaList<BookmarkTag>> PublicSelectedTagsProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, AvaloniaList<BookmarkTag>>(nameof(PublicSelectedTags), t => t.PublicSelectedTags,
            (t, v) => t.PublicSelectedTags = v);

    public AvaloniaList<BookmarkTag> PublicSelectedTags
    {
        get;
        set
        {
            var oldCurrentSelectedTags = CurrentSelectedTags;
            SetAndRaise(PublicSelectedTagsProperty, ref field, value);
            if (!IsPrivate)
                RaisePropertyChanged(CurrentSelectedTagsProperty, oldCurrentSelectedTags, CurrentSelectedTags);
        }
    } = [];

    public static readonly DirectProperty<TagSelector, AvaloniaList<BookmarkTag>> PrivateSelectedTagsProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, AvaloniaList<BookmarkTag>>(nameof(PrivateSelectedTags), t => t.PrivateSelectedTags,
            (t, v) => t.PrivateSelectedTags = v);

    public AvaloniaList<BookmarkTag> PrivateSelectedTags
    {
        get;
        set
        {
            var oldCurrentSelectedTags = CurrentSelectedTags;
            SetAndRaise(PrivateSelectedTagsProperty, ref field, value);
            if (IsPrivate)
                RaisePropertyChanged(CurrentSelectedTagsProperty, oldCurrentSelectedTags, CurrentSelectedTags);
        }
    } = [];

    public static readonly DirectProperty<TagSelector, AvaloniaList<BookmarkTag>> CurrentTagsSourceProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, AvaloniaList<BookmarkTag>>(nameof(CurrentTagsSource), t => t.CurrentTagsSource);

    public AvaloniaList<BookmarkTag> CurrentTagsSource
    {
        get => IsPrivate ? PrivateTagsSource : PublicTagsSource;
    }

    public static readonly DirectProperty<TagSelector, AvaloniaList<BookmarkTag>> CurrentSelectedTagsProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, AvaloniaList<BookmarkTag>>(nameof(CurrentSelectedTags), t => t.CurrentSelectedTags);

    public AvaloniaList<BookmarkTag> CurrentSelectedTags
    {
        get => IsPrivate ? PrivateSelectedTags : PublicSelectedTags;
    }

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
        set
        {
            if (field == value)
                return;

            var oldCurrentTagsSource = CurrentTagsSource;
            var oldCurrentSelectedTags = CurrentSelectedTags;
            SetAndRaise(IsPrivateProperty, ref field, value);
            RaisePropertyChanged(CurrentTagsSourceProperty, oldCurrentTagsSource, CurrentTagsSource);
            RaisePropertyChanged(CurrentSelectedTagsProperty, oldCurrentSelectedTags, CurrentSelectedTags);
        }
    }

    public TagSelector()
    {
        InitializeComponent();
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        TagsSelected?.Invoke(this,
            new ValueTuple<bool, IReadOnlyList<string>>(IsPrivate, IsPrivate
                ? [.. PrivateSelectedTags.Select(t => t.Name)]
                : [.. PublicSelectedTags.Select(t => t.Name)]));
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox)
            foreach (var item in e.AddedItems.OfType<AddNewBookmarkTag>())
                listBox.SelectedItems?.Remove(item);
    }

    public async Task ResetSourceAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        PublicSelectedTags.Clear();
        PrivateSelectedTags.Clear();
        PublicTagsSource = [];
        PrivateTagsSource = [];

        try
        {
            var publicTagsTask = GetTagsAsync(PrivacyPolicy.Public);
            var privateTagsTask = GetTagsAsync(PrivacyPolicy.Private);
            await Task.WhenAll(publicTagsTask, privateTagsTask);

            PublicTagsSource = publicTagsTask.Result;
            PrivateTagsSource = privateTagsTask.Result;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<AvaloniaList<BookmarkTag>> GetTagsAsync(PrivacyPolicy policy)
    {
        var tags = await App.AppViewModel.MakoClient.WorkBookmarkTags(App.AppViewModel.PixivUid, WorkType, policy).ToListAsync();
        return new AvaloniaList<BookmarkTag>(tags)
        {
            new AddNewBookmarkTag
            {
                Name = "",
                Count = 0,
                TagAdded = (_, name) => AddTag(policy, name)
            }
        };
    }

    private void AddTag(PrivacyPolicy policy, string name)
    {
        var tagsSource = policy is PrivacyPolicy.Private ? PrivateTagsSource : PublicTagsSource;
        var selectedTags = policy is PrivacyPolicy.Private ? PrivateSelectedTags : PublicSelectedTags;
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
