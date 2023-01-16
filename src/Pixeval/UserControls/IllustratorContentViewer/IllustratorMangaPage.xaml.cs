using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.UserControls.IllustrationView;
using Pixeval.Util;
using Pixeval.Utilities;
using System.Linq;
using Pixeval.CoreApi.Model;

namespace Pixeval.UserControls.IllustratorContentViewer;

public sealed partial class IllustratorMangaPage : ISortedIllustrationContainerPageHelper
{
    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public IllustratorMangaPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        WeakReferenceMessenger.Default.Register<IllustratorMangaPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustrationContainer.ViewModel.DataProvider.FetchEngine?.Cancel());
        if (e.Parameter is string id)
        {
            IllustrationContainer.IllustrationView.ViewModel.DataProvider.ResetAndFillAsync(App.AppViewModel.MakoClient.Posts(id));
        }
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((ISortedIllustrationContainerPageHelper) this).OnSortOptionChanged();
    }

    private void SortOptionComboBoxContainer_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (App.AppViewModel.AppSetting.IllustrationViewOption is IllustrationViewOption.Justified)
        {
            ToolTipService.SetToolTip(SortOptionComboBoxContainer, new ToolTip { Content = MiscResources.SortIsNotAllowedWithJustifiedLayout });
        }
    }

    public void Dispose()
    {
        IllustrationContainer.IllustrationView.ViewModel.Dispose();
    }
}