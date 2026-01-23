using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.Utilities;
using Frame = FluentAvalonia.UI.Controls.Frame;

namespace Pixeval;

public partial class BookmarksPage : UserControl
{
    private long _userId;
    private bool _suppressChangeSource;

    public static IReadOnlyList<BookmarkTag> DefaultTags { get; }= [AllBookmarkTag.Instance];

    public BookmarksPage()
    {
        InitializeComponent();

        AddHandler(Frame.NavigatedToEvent, (sender, e) =>
        {
            if (e.Parameter is not long uid)
                uid = App.AppViewModel.PixivUid;
            else if (uid != App.AppViewModel.PixivUid)
                PrivacyPolicyComboBox.IsEnabled =  PrivacyPolicyComboBox.IsVisible = false;

            _userId = uid;
            FetchTags();
            ChangeSource();
        });
    }

    private void WorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        FetchTags();
        ChangeSource();
    }

    private void TagComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _suppressChangeSource)
            return;

        ChangeSource();
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    public async void FetchTags()
    {
        var tags = await MakoHelper.GetBookmarkTagsAsync(
            _userId,
            SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>(),
            PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>());

        _suppressChangeSource = true;
        TagComboBox.ItemsSource = tags;
        TagComboBox.SelectedIndex = 0;
        _suppressChangeSource = false;
    }

    private void ChangeSource()
    {
        var tag = (TagComboBox.SelectedItem as BookmarkTag)?.Name;
        WorkContainer.ResetEngine(App.AppViewModel.MakoClient.WorkBookmarks(
            _userId,
            SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>(),
            PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>(),
            tag));
    }
}
