using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Capability;

namespace Pixeval.Views;

public partial class LoginPage : UserControl
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var token = TextBox.Text;
        if (string.IsNullOrWhiteSpace(token))
            return;

        App.AppViewModel.MakoClient.SetToken(token);
        if (await App.AppViewModel.MakoClient.IdentifyTokenAsync())
        {
            var loginContext = App.AppViewModel.LoginContext;
            loginContext.CurrentRefreshToken = token;
            AppInfo.SaveLoginContext(loginContext);

            var viewContainer = TopLevel.GetTopLevel(this)?.ViewContainer;
            viewContainer?.NavigateTo<RecommendWorksPage>(true);
        }
    }
}
