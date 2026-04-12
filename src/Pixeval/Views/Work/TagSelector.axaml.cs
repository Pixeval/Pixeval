using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Pixeval.Views.Work;

public class TagSelector : TemplatedControl
{
    public event EventHandler<TagSelector, (bool IsPrivate, IReadOnlyList<string> Tags)>? TagsSelected; 

    public static readonly DirectProperty<TagSelector, SimpleWorkType> WorkTypeProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, SimpleWorkType>(nameof(WorkType), t => t.WorkType, (t, v) => t.WorkType = v);

    public SimpleWorkType WorkType
    {
        get;
        set => SetAndRaise(WorkTypeProperty, ref field, value);
    }
 
    public static readonly DirectProperty<TagSelector, bool> IsPrivateProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, bool>(nameof(IsPrivate), t => t.IsPrivate, (t, v) => t.IsPrivate = v);

    public bool IsPrivate
    {
        get;
        private set => SetAndRaise(IsPrivateProperty, ref field, value);
    }

    public static readonly DirectProperty<TagSelector, AvaloniaList<BookmarkTag>> TagsSourceProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, AvaloniaList<BookmarkTag>>(nameof(TagsSource), t => t.TagsSource, (t, v) => t.TagsSource = v);

    public AvaloniaList<BookmarkTag> TagsSource
    {
        get;
        private set => SetAndRaise(TagsSourceProperty, ref field, value);
    } = [];

    public static readonly DirectProperty<TagSelector, AvaloniaList<BookmarkTag>> SelectedTagsProperty =
        AvaloniaProperty.RegisterDirect<TagSelector, AvaloniaList<BookmarkTag>>(nameof(SelectedTags), t => t.SelectedTags, (t, v) => t.SelectedTags = v);

    public AvaloniaList<BookmarkTag> SelectedTags
    {
        get;
        private set => SetAndRaise(SelectedTagsProperty, ref field, value);
    } = [];

    private ListBox? _listBox;

    private Button? _confirmButton;
    private Button? _cancelButton;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _listBox?.SelectionChanged -= OnListBoxSelectionChanged;
        _confirmButton?.Click -= OnConfirmClick;
        _confirmButton?.Click -= OnCancelClick;
        _cancelButton?.Click -= OnCancelClick;
        _listBox = e.NameScope.Find<ListBox>("PART_ListBox");
        _confirmButton = e.NameScope.Find<Button>("PART_ConfirmButton");
        _cancelButton = e.NameScope.Find<Button>("PART_CancelButton");
        _listBox?.ContainerPrepared += OnListBoxSelectionChanged;
        _confirmButton?.Click += OnConfirmClick;
        _confirmButton?.Click += OnCancelClick;
        _cancelButton?.Click += OnCancelClick;
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e) =>
        TagsSelected?.Invoke(this, (IsPrivate, [.. SelectedTags.Select(t => t.Name)]));

    private void OnCancelClick(object? sender, RoutedEventArgs e) => IsVisible = false;

    private static void OnListBoxSelectionChanged(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is ListBoxItem { DataContext: AddNewBookmarkTag } container)
        {
            container.Theme = new ControlTheme(typeof(ListBoxItem))
            {
                Setters =
                {
                    new Setter(TemplateProperty, new FuncControlTemplate<ListBoxItem>((item, _) =>
                        new ContentPresenter
                        {
                            [~ContentPresenter.ContentProperty] = item[~ContentControl.ContentProperty],
                            [~ContentPresenter.ContentTemplateProperty] = item[~ContentControl.ContentTemplateProperty]
                        }))
                }
            };
        }
    }

    private static void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox)
            foreach (var item in e.AddedItems.OfType<AddNewBookmarkTag>())
                listBox.SelectedItems?.Remove(item);
    }

    /// <inheritdoc />
    protected override async void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IsPrivateProperty)
            await ResetSourceAsync();
    }

    public async Task ResetSourceAsync()
    {
        var policy = IsPrivate ? PrivacyPolicy.Private : PrivacyPolicy.Public;
        var tags = await App.AppViewModel.MakoClient.WorkBookmarkTag(App.AppViewModel.PixivUid, WorkType, policy).ToListAsync();
        var list = new AvaloniaList<BookmarkTag>(tags)
        {
            new AddNewBookmarkTag
            {
                Name = "",
                Count = 0,
                TagAdded = (_, name) =>
                {
                    if (TagsSource.All(t => t.Name != name))
                    {
                        var newTag = new BookmarkTag
                        {
                            Name = name,
                            Count = 0
                        };
                        TagsSource.Insert(TagsSource.Count - 1, newTag);
                        SelectedTags.Add(newTag);
                    }
                }
            }
        };
        SelectedTags.Clear();
        TagsSource = list;
    }
}
