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
    protected SimpleUsersPage(string header, Symbol symbol)
    {
        Header = header;
        Icon = new SymbolIcon { Symbol = symbol, FontSize = 16, IconVariant = IconVariant.Color };
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

public class UserRecommendPage : SimpleUsersPage
{
    public UserRecommendPage() : this(null)
    {
    }

    public UserRecommendPage(UserViewViewModel? viewModel) : base(I18NManager.GetResource(MainPageResources.TabUserRecommended), Symbol.PeopleCommunity)
    {
        InitializeSource(viewModel);
    }

    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.UserRecommended();
    }
}

public class UserSearchPage : SimpleUsersPage
{
    private readonly string? _searchText;

    public UserSearchPage() : this(null)
    {
    }

    public UserSearchPage(string? searchText, UserViewViewModel? viewModel = null) : base(I18NManager.GetResource(MainPageResources.SearchResultFormatted, searchText), Symbol.Person)
    {
        _searchText = searchText;
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

public class UserMyPixivPage : SimpleUsersPage
{
    private readonly long _userId;

    public UserMyPixivPage() : this(App.AppViewModel.PixivUid)
    {
    }

    public UserMyPixivPage(long id, UserViewViewModel? viewModel = null) : base(I18NManager.GetResource(MainPageResources.TabUserMyPixiv), Symbol.People)
    {
        _userId = id;
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

    public RelatedUsersPage(long id) : base(I18NManager.GetResource(MainPageResources.TabRelatedUser), Symbol.PeopleCommunity)
    {
        _userId = id;
        ChangeSource();
    }

    /// <inheritdoc />
    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.Computed(makoClient.RelatedUserAsync(_userId).ToAsyncEnumerable());
    }
}
