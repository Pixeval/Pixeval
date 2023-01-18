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
using Pixeval.Util.Threading;

namespace Pixeval.UserControls.IllustratorContentViewer;

public sealed partial class IllustratorMangaPage : ISortedIllustrationContainerPageHelper, IIllustratorContentViewerCommandBarHostSubPage
{
    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public IllustratorMangaPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (ActivationCount > 1) return;

        WeakReferenceMessenger.Default.Register<IllustratorMangaPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustrationContainer.ViewModel.DataProvider.FetchEngine?.Cancel());
        if (e.Parameter is string id)
        {
            IllustrationContainer.IllustrationView.ViewModel.ResetEngineAndFillAsync(App.AppViewModel.MakoClient.MangaPosts(id, App.AppViewModel.AppSetting.TargetFilter)).Discard();
        }

        if (!App.AppViewModel.AppSetting.ShowExternalCommandBarInIllustratorContentViewer)
        {
            ChangeCommandBarVisibility(false);
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

    public void PerformSearch(string keyword)
    {
        if (IllustrationContainer.ShowCommandBar)
        {
            return;
        }

        if (keyword.IsNullOrBlank())
        {
            IllustrationContainer.IllustrationView.ViewModel.DataProvider.Filter = null;
        }
        else
        {
            IllustrationContainer.IllustrationView.ViewModel.DataProvider.Filter = o =>
            {
                if (o is IllustrationViewModel viewModel)
                {
                    return viewModel.Id.Contains(keyword)
                           || (viewModel.Illustration.Tags ?? Enumerable.Empty<Tag>()).Any(x => x.Name.Contains(keyword) || (x.TranslatedName?.Contains(keyword) ?? false))
                           || (viewModel.Illustration.Title?.Contains(keyword) ?? false);
                }

                return false;
            };
        }
    }

    public void ChangeCommandBarVisibility(bool isVisible)
    {
        IllustrationContainer.ShowCommandBar = isVisible;
    }
}