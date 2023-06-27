using System;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.UserControls.IllustratorContentViewer;
using Pixeval.UserControls.IllustratorView;

namespace Pixeval.Pages.Misc;

public sealed partial class IllustratorContentViewerPage : IDisposable
{
    private IllustratorContentViewerViewModel? _illustratorContentViewerViewModel;

    public IllustratorContentViewerPage()
    {
        InitializeComponent();
    }

    public override async void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is IllustratorViewModel viewModel)
        {
            _illustratorContentViewerViewModel = new IllustratorContentViewerViewModel(viewModel.UserDetail ?? await App.AppViewModel.MakoClient.GetUserFromIdAsync(viewModel.UserId!, App.AppViewModel.AppSetting.TargetFilter));
            IllustratorContentViewer.ViewModel = _illustratorContentViewerViewModel;
        }
    }

    public void Dispose()
    {
        IllustratorContentViewer.Dispose();
    }
}