using System.Collections.ObjectModel;
using Mako.Engine;
using Mako.Model;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    // TODO : change IllustrationGridPage to UserControl
    public sealed partial class IllustrationGridPage
    {
        private readonly IllustrationGridPageViewModel _viewModel = new();

        public IllustrationGridPage()
        {
            AdditionalCommandBarButtons = additionalCommandBarButtons;
            InitializeComponent();
        }

        public ObservableCollection<ICommandBarElement>? AdditionalCommandBarButtons { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await _viewModel.Fill(e.Parameter as IFetchEngine<Illustration?>);
        }

        private async void RemoveBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var viewModel = sender.GetDataContext<IllustrationViewModel>();
            await viewModel!.RemoveBookmarkAsync();
        }

        private async void PostBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var viewModel = sender.GetDataContext<IllustrationViewModel>();
            await viewModel!.PostPublicBookmarkAsync();
        }
    }
}
