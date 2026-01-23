using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako;
using Mako.Engine;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Frame = FluentAvalonia.UI.Controls.Frame;

namespace Pixeval.Views.Capability;

public abstract partial class SimpleUsersPage : UserControl
{
    public SimpleUsersPage()
    {
        InitializeComponent();
        AddHandler(Frame.NavigatedToEvent, (sender, e) => ChangeSource());
    }

    private void UserContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        (UserContainer.UserView.DataContext as UserViewViewModel)?.ResetEngine(GetFetchEngine(App.AppViewModel.MakoClient));
    }

    protected abstract IFetchEngine<User> GetFetchEngine(MakoClient makoClient);
}

public class RecommendUsersPage : SimpleUsersPage
{
    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.RecommendedUsers(PixevalSettings.TargetFilter);
    }
}

public class SearchUsersPage : SimpleUsersPage
{
    private string? _searchText;

    public SearchUsersPage()
    {
        AddHandler(Frame.NavigatedToEvent, (sender, e) =>
        {
            if (e.Parameter is string s)
                _searchText = s;
        });
    }

    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        if (_searchText is null)
            return makoClient.Computed(AsyncEnumerable.Empty<User>());
        return App.AppViewModel.MakoClient.SearchUser(
            _searchText,
            PixevalSettings.TargetFilter);
    }
}

public class MyPixivUsersPage : SimpleUsersPage
{
    private long _userId;

    public MyPixivUsersPage()
    {
        AddHandler(Frame.NavigatedToEvent, (sender, e) =>
        {
            if (e.Parameter is not long uid)
                uid = App.AppViewModel.PixivUid;

            _userId = uid;
        });
    }

    /// <inheritdoc />
    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.MyPixivUsers(_userId);
    }
}

public class RelatedUsersPage : SimpleUsersPage
{
    private long _userId;

    public RelatedUsersPage()
    {
        AddHandler(Frame.NavigatedToEvent, (sender, e) =>
        {
            if (e.Parameter is not long uid)
                uid = App.AppViewModel.PixivUid;

            _userId = uid;
        });
    }

    /// <inheritdoc />
    protected override IFetchEngine<User> GetFetchEngine(MakoClient makoClient)
    {
        return makoClient.Computed(makoClient.RelatedUserAsync(_userId, App.AppViewModel.AppSettings.TargetFilter).ToAsyncEnumerable());
    }
}
