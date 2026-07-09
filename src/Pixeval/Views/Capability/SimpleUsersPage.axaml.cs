// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia.Interactivity;
using Mako;
using Mako.Engine;
using Mako.Model;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public abstract partial class SimpleUsersPage : IconContentPage
{
    protected SimpleUsersPage() => InitializeComponent();

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

public class UserRecommendedPage : SimpleUsersPage
{
    public UserRecommendedPage() : this(null)
    {
    }

    public UserRecommendedPage(UserViewViewModel? viewModel)
    {
        InitializeSource(viewModel);
    }

    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.UserRecommended();
    }
}

public class UserSearchResultPage : SimpleUsersPage
{
    private readonly string? _searchText;

    public UserSearchResultPage() : this(null)
    {
    }

    public UserSearchResultPage(string? searchText, UserViewViewModel? viewModel = null)
    {
        Header = I18NManager.GetResource(MainPageResources.SearchResultFormatted, searchText);
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

public class UserFollowerPage : SimpleUsersPage
{
    public UserFollowerPage() : this(null)
    {
    }

    public UserFollowerPage(UserViewViewModel? viewModel)
    {
        InitializeSource(viewModel);
    }

    /// <inheritdoc />
    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.UserFollower();
    }
}

public class UserMyPixivPage : SimpleUsersPage
{
    private readonly long _userId;

    public UserMyPixivPage() : this(PixevalSettings.MyId)
    {
    }

    public UserMyPixivPage(long id, UserViewViewModel? viewModel = null)
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

    public RelatedUsersPage() : this(PixevalSettings.MyId)
    {
    }

    public RelatedUsersPage(long id)
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
