// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.UI.Xaml;

namespace Pixeval.Pages.Capability;

public sealed partial class BookmarksPage : IScrollViewHost
{
    private BookmarksPageViewModel _viewModel = null!;

    private long _uid;

    public BookmarksPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (e.Parameter is not long uid)
            uid = App.AppViewModel.PixivUid;
        _uid = uid;
        _viewModel = new BookmarksPageViewModel(uid);
        ChangeSource();
    }

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();


    private bool BookmarkTagFilter(string name, IWorkViewModel viewModel) => viewModel.Entry is IWorkEntry entry && _viewModel.ContainsTag(name, entry.Id);


    private async void ChangeSource()
    {
        WorkContainer.WorkView.ResetEngine(await App.AppViewModel.GetEngineAsync<Illustration>("favorite/list", _uid));
    }

    private void SetFilter(Func<IWorkViewModel, bool>? filter)
    {
        if (WorkContainer.ViewModel is { } vm)
            vm.Filter = filter;
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;

    private void WorkContainer_OnRefreshRequested(object sender, RoutedEventArgs e) => ChangeSource();
}
