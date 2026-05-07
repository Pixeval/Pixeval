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
    public RecommendUsersPage()
    {
        Header = I18NManager.GetResource(MainPageResources.RecommendUsersTabContent);
        Icon = new SymbolIcon { Symbol = Symbol.PeopleCommunity, FontSize = 16, IconVariant = IconVariant.Color };
        ChangeSource();
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

    public SearchUsersPage(string? searchText)
    {
        _searchText = searchText;
        Header = I18NManager.GetResource(MainPageResources.SearchResultFormatted, _searchText);
        Icon = new SymbolIcon { Symbol = Symbol.Person, FontSize = 16, IconVariant = IconVariant.Color };
        ChangeSource();
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

    public MyPixivUsersPage(long id)
    {
        _userId = id;
        Header = I18NManager.GetResource(EntryViewerPageResources.MyPixivUserNavigationViewItemContent);
        Icon = new SymbolIcon { Symbol = Symbol.People, FontSize = 16, IconVariant = IconVariant.Color };
        ChangeSource();
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
