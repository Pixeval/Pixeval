// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Mako;
using Mako.Engine;
using Mako.Model;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public abstract partial class SimpleUsersPage : ContentPage
{
    public SimpleUsersPage()
    {
        InitializeComponent();
    }

    protected void InitializeSource(UserViewViewModel? viewModel = null)
    {
        if (viewModel is null)
        {
            ChangeSource();
            return;
        }

        var oldViewModel = UserContainer.UserView.DataContext as IDisposable;
        UserContainer.UserView.DataContext = viewModel;
        oldViewModel?.Dispose();
    }

    private void UserContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    protected void ChangeSource()
    {
        (UserContainer.UserView.DataContext as UserViewViewModel)?.ResetEngine(GetFetchEngine(App.AppViewModel.MakoClient), (user, _) => new(user));
    }

    protected abstract IFetchEngine<User> GetFetchEngine(MakoClient makoClient);
}

public class RecommendUsersPage : SimpleUsersPage
{
    public RecommendUsersPage() : this(null)
    {
    }

    public RecommendUsersPage(UserViewViewModel? viewModel)
    {
        Header = I18NManager.GetResource(MainPageResources.RecommendUsersTabContent);
        Icon = new SymbolIcon { Symbol = Symbol.PeopleCommunity, FontSize = 16, IconVariant = IconVariant.Color };
        InitializeSource(viewModel);
    }

    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.UserRecommended();
    }
}

public class SearchUsersPage : SimpleUsersPage
{
    private readonly string? _searchText;

    public SearchUsersPage() : this(null)
    {
    }

    public SearchUsersPage(string? searchText, UserViewViewModel? viewModel = null)
    {
        _searchText = searchText;
        Header = I18NManager.GetResource(MainPageResources.SearchResultFormatted, _searchText);
        Icon = new SymbolIcon { Symbol = Symbol.Person, FontSize = 16, IconVariant = IconVariant.Color };
        InitializeSource(viewModel);
    }

    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        if (_searchText is null)
            return makoClient.Computed(AsyncEnumerable.Empty<User>());
        return App.AppViewModel.MakoClient.UserSearch(
            _searchText);
    }
}

public class MyPixivUsersPage : SimpleUsersPage
{
    private readonly long _userId;

    public MyPixivUsersPage() : this(App.AppViewModel.PixivUid)
    {
    }

    public MyPixivUsersPage(long id, UserViewViewModel? viewModel = null)
    {
        _userId = id;
        Header = I18NManager.GetResource(EntryViewerPageResources.MyPixivUserNavigationViewItemContent);
        Icon = new SymbolIcon { Symbol = Symbol.People, FontSize = 16, IconVariant = IconVariant.Color };
        InitializeSource(viewModel);
    }

    /// <inheritdoc />
    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.UserMyPixiv(_userId);
    }
}

public class RelatedUsersPage : SimpleUsersPage
{
    private readonly long _userId;

    public RelatedUsersPage() : this(App.AppViewModel.PixivUid)
    {
    }

    public RelatedUsersPage(long id)
    {
        _userId = id;
        Header = I18NManager.GetResource(EntryViewerPageResources.RelatedUserNavigationViewItemContent);
        Icon = new SymbolIcon { Symbol = Symbol.PeopleCommunity, FontSize = 16, IconVariant = IconVariant.Color };
        ChangeSource();
    }

    /// <inheritdoc />
    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.Computed(makoClient.RelatedUserAsync(_userId).ToAsyncEnumerable());
    }
}
