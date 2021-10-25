using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Download;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.UserControls;

namespace Pixeval.Pages.Download
{
    public sealed partial class DownloadListEntry
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ObservableDownloadTask),
            typeof(DownloadListEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public ObservableDownloadTask ViewModel
        {
            get => (ObservableDownloadTask) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register(
            nameof(Thumbnail),
            typeof(ImageSource),
            typeof(DownloadListEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, ThumbnailChanged));

        public ImageSource Thumbnail
        {
            get => (ImageSource)GetValue(ThumbnailProperty);
            set => SetValue(ThumbnailProperty, value);
        }

        private static void ThumbnailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DownloadListEntry { ThumbnailImage: var image } && e.NewValue is ImageSource value)
            {
                image.Source = value;
            }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(DownloadListEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(DownloadListEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public string Description
        {
            get => (string) GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            nameof(Progress),
            typeof(double),
            typeof(DownloadListEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, ProgressChanged));

        public double Progress
        {
            get => (double) GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DownloadListEntry { DownloadProgressBar: var progressBar } && e.NewValue is double value)
            {
                progressBar.Value = value;
            }
        }

        public static readonly DependencyProperty ProgressMessageProperty = DependencyProperty.Register(
            nameof(ProgressMessage),
            typeof(string),
            typeof(DownloadListEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, ProgressMessageChanged));

        public string ProgressMessage
        {
            get => (string) GetValue(ProgressMessageProperty);
            set => SetValue(ProgressMessageProperty, value);
        }

        private static void ProgressMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DownloadListEntry { ProgressMessageTextBlock: var block } && e.NewValue is string value)
            {
                block.Text = value;
            }
        }

        public static readonly DependencyProperty ActionButtonContentProperty = DependencyProperty.Register(
            nameof(ActionButtonContent),
            typeof(string),
            typeof(DownloadListEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, ActionButtonContentChanged));

        public string ActionButtonContent
        {
            get => (string) GetValue(ActionButtonContentProperty);
            set => SetValue(ActionButtonContentProperty, value);
        }

        private static void ActionButtonContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DownloadListEntry { ActionButton: var button } && e.NewValue is string value)
            {
                button.Content = value;
            }
        }

        public static readonly DependencyProperty IsCancelItemEnabledProperty = DependencyProperty.Register(
            nameof(IsCancelItemEnabled),
            typeof(bool),
            typeof(DownloadListEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, IsCancelButtonEnabledChanged));

        public bool IsCancelItemEnabled
        {
            get => (bool) GetValue(IsCancelItemEnabledProperty);
            set => SetValue(IsCancelItemEnabledProperty, value);
        }

        private static void IsCancelButtonEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DownloadListEntry { CancelDownloadItem: var item } && e.NewValue is bool value)
            {
                item.IsEnabled = value;
            }
        }

        public static readonly DependencyProperty ActionButtonBackgroundProperty = DependencyProperty.Register(
            nameof(ActionButtonBackground),
            typeof(Brush),
            typeof(DownloadListEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, ActionButtonBackgroundChanged));

        public Brush ActionButtonBackground
        {
            get => (Brush) GetValue(ActionButtonBackgroundProperty);
            set => SetValue(ActionButtonBackgroundProperty, value);
        }

        private static void ActionButtonBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DownloadListEntry { ActionButton: var button } && e.NewValue is Brush value)
            {
                button.Background = value;
            }
        }

        public DownloadListEntry()
        {
            InitializeComponent();
        }

        private void DownloadListEntry_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            const int minWindowWidth = 768;
            switch (e.PreviousSize.Width)
            {
                case > minWindowWidth when e.NewSize.Width <= minWindowWidth:
                    ApplyVisualStateChange(true);
                    break;
                case <= minWindowWidth when e.NewSize.Width > minWindowWidth:
                    ApplyVisualStateChange(false);
                    break;
            }
        }

        private void ApplyVisualStateChange(bool compact)
        {
            if (compact)
            {
                ImageColumn.Width = new GridLength(1, GridUnitType.Star);
                CaptionColumn.Width = new GridLength(0);
                ProgressColumn.Width = new GridLength(0);
                ButtonColumn.Width = new GridLength(0);
                OptionColumn.Width = new GridLength(0);
                Grid.SetRowSpan(ThumbnailImageContainer, 1);
                Grid.SetRowSpan(CaptionContainer, 1);
                Grid.SetColumn(CaptionContainer, 0);
                CaptionContainer.Margin = new Thickness(60, 0, 0, 0);
                CaptionContainer.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetRow(ProgressBarContainer, 1);
                Grid.SetRowSpan(ProgressBarContainer, 1);
                Grid.SetColumn(ProgressBarContainer, 0);
                ProgressBarContainer.Margin = new Thickness(0, 8, 0, 0);
                Grid.SetRow(ActionButton, 2);
                Grid.SetRowSpan(ActionButton, 1);
                Grid.SetColumn(ActionButton, 0);
                ActionButton.Width = double.NaN;
                ActionButton.HorizontalAlignment = HorizontalAlignment.Stretch;
                ActionButton.Margin = new Thickness(0, 8, 0, 0);
                Grid.SetRowSpan(MoreOptionButton, 1);
                Grid.SetColumn(MoreOptionButton, 0);
                MoreOptionButton.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                ImageColumn.Width = new GridLength(60);
                CaptionColumn.Width = new GridLength(120);
                ProgressColumn.Width = new GridLength(1, GridUnitType.Star);
                ButtonColumn.Width = new GridLength(130);
                OptionColumn.Width = new GridLength(45);
                Grid.SetRowSpan(ThumbnailImageContainer, 3);
                Grid.SetRowSpan(CaptionContainer, 3);
                Grid.SetColumn(CaptionContainer, 1);
                CaptionContainer.Margin = new Thickness(0);
                CaptionContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
                Grid.SetRow(ProgressBarContainer, 0);
                Grid.SetRowSpan(ProgressBarContainer, 3);
                Grid.SetColumn(ProgressBarContainer, 2);
                ProgressBarContainer.Margin = new Thickness(0, 3, 50, 0);
                Grid.SetRow(ActionButton, 0);
                Grid.SetRowSpan(ActionButton, 3);
                Grid.SetColumn(ActionButton, 3);
                ActionButton.Width = 120;
                ActionButton.HorizontalAlignment = HorizontalAlignment.Center;
                ActionButton.Margin = new Thickness(0);
                Grid.SetRowSpan(MoreOptionButton, 3);
                Grid.SetColumn(MoreOptionButton, 4);
                MoreOptionButton.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        private void ActionButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            switch (ViewModel.CurrentState)
            {
                case DownloadState.Created:
                case DownloadState.Queued:
                    ViewModel.CancellationHandle.Cancel();
                    break;
                case DownloadState.Running:
                    ViewModel.CancellationHandle.Pause();
                    break;
                case DownloadState.Error:
                case DownloadState.Cancelled:
                case DownloadState.Completed:
                    ViewModel.Reset();
                    App.AppViewModel.DownloadManager.TryExecuteInline(ViewModel);
                    break;
                case DownloadState.Paused:
                    ViewModel.CancellationHandle.Resume();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CancelDownloadItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.CancellationHandle.Cancel();
        }

        private void OpenDownloadLocationItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Process.Start("explorer.exe", $@"/select, ""{ViewModel.Destination}""");
        }

        private void RemoveFromListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.CancellationHandle.Cancel();
            App.AppViewModel.DownloadManager.RemoveTask(ViewModel);
        }

        private async void GoToPageItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            switch (ViewModel)
            {
                case IllustrationDownloadTask {IllustrationViewModel: var illustrationViewModel}:
                    var viewModels = (!illustrationViewModel.IsManga ? illustrationViewModel : new IllustrationViewModel(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(illustrationViewModel.Id)))
                        .GetMangaIllustrationViewModels()
                        .ToArray();

                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", App.AppViewModel.AppWindowRootFrame);
                    App.AppViewModel.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModels), new SuppressNavigationTransitionInfo());
                    break;
            }
        }
    }
}
