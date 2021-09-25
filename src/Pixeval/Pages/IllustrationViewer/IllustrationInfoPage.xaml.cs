using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;
using Pixeval.Util;
using Pixeval.ViewModel;
using ReverseMarkdown;

namespace Pixeval.Pages.IllustrationViewer
{
    public sealed partial class IllustrationInfoPage
    {
        private IllustrationViewerPageViewModel _viewModel = null!;

        public IllustrationInfoPage()
        {
            InitializeComponent();
        }

        public override void Prepare(NavigationEventArgs e)
        {
            _viewModel = (IllustrationViewerPageViewModel) e.Parameter;
            SetIllustrationCaptionText();
        }

        private void IllustrationTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO
            // throw new System.NotImplementedException();
        }

        private async void IllustrationCaptionMarkdownTextBlock_OnLinkClicked(object? sender, LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void IllustratorPersonPicture_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // TODO
            // throw new NotImplementedException();
        }

        #region Helper Functions

        private IEnumerable<Tag> GetIllustrationTagItemSource()
        {
            return _viewModel.Current.IllustrationViewModel.Illustration.Tags ?? Enumerable.Empty<Tag>();
        }

        public static string GetMakoTagTranslatedNameText(string? name, string? fallback)
        {
            return (name.IsNullOrEmpty() ? fallback : name) ?? string.Empty;
        }

        private string GetIllustratorNameText()
        {
            return IllustrationInfoPageResources.IllustratorNameFormatted.Format(_viewModel.IllustratorName);
        }

        private string GetIllustratorIdText()
        {
            return IllustrationInfoPageResources.IllustratorNameFormatted.Format(_viewModel.IllustratorUid);
        }

        private string GetIllustrationDimensionText()
        {
            return _viewModel.Current.IllustrationViewModel.Illustration.Let(i => $"{i.Width} x {i.Height}") ?? IllustrationInfoPageResources.IllustrationDimensionUnknown;
        }

        private string GetIllustrationUploadDateText()
        {
            return _viewModel.Current.IllustrationViewModel.Illustration.CreateDate.ToString("yyyy-M-d HH:mm:ss");
        }

        private readonly Converter _markdownConverter = new(new Config
        {
            UnknownTags = Config.UnknownTagsOption.PassThrough,
            GithubFlavored = true
        });

        private void SetIllustrationCaptionText()
        {
            var caption = _viewModel.Current.IllustrationViewModel.Illustration.Caption;
            Task.Run(() => string.IsNullOrEmpty(caption) ? IllustrationInfoPageResources.IllustrationCaptionEmpty : _markdownConverter.Convert(caption))
                .ContinueWith(task => IllustrationCaptionMarkdownTextBlock.Text = task.Result, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion
    }
}
