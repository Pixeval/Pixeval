using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Windows.Foundation;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    public sealed partial class ImageViewerPage
    {
        private ImageViewerPageViewModel _viewModel = null!;

        private const int MaxZoomFactor = 8;

        private const int MinZoomFactor = 1;

        public ImageViewerPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _viewModel = (ImageViewerPageViewModel) e.Parameter;
            if (_viewModel.IllustrationViewModel.ThumbnailSource is null)
            {
                _ = _viewModel.IllustrationViewModel.LoadThumbnail().ContinueWith(_ =>
                {
                    DispatcherQueue.TryEnqueue(() => SetSource(_viewModel.IllustrationViewModel.ThumbnailSource!));
                });
            }

            SetSource(await _viewModel.LoadOriginalImage());
        }

        private void IllustrationOriginalImage_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        { 
            var (deltaX, deltaY) = (e.Delta.Translation.X, e.Delta.Translation.Y);
            if (GetZoomFactor() > 1)
            {
                var renderedImageWidth = IllustrationOriginalImage.ActualWidth * GetZoomFactor();
                var renderedImageHeight = IllustrationOriginalImage.ActualHeight * GetZoomFactor();
                var containerWidth = IllustrationOriginalImageContainer.ActualWidth;
                var containerHeight = IllustrationOriginalImageContainer.ActualHeight;
                var pos = IllustrationOriginalImage.TransformToVisual(IllustrationOriginalImageContainer).TransformPoint(new Point(0, 0));
                if (renderedImageWidth > containerWidth)
                {
                    switch (deltaX)
                    {
                        case < 0 when pos.X > -(renderedImageWidth - containerWidth):
                        case > 0 when pos.X < 0:
                            IllustrationOriginalImageRenderTransform.TranslateX += deltaX;
                            break;
                    }
                }

                if (renderedImageHeight > containerHeight)
                {
                    switch (deltaY)
                    {
                        case < 0 when pos.Y > -(renderedImageHeight - containerHeight):
                        case > 0 when pos.Y < 0:
                            IllustrationOriginalImageRenderTransform.TranslateY += deltaY;
                            break;
                    }
                }
            }
        }

        private void IllustrationOriginalImage_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (GetZoomFactor() > 1)
            {
                ResetImageScaleStoryboard.Begin();
                ResetImageTranslationStoryboard.Begin();
                if (_tappedScaled)
                {
                    _tappedScaled.Inverse();
                }
            }
            else
            {
                ImageScaledOut200PercentStoryboard.Begin();
                _tappedScaled.Inverse();
            }
        }

        private void ResetImageScaleStoryboard_OnCompleted(object? sender, object e)
        {
            ResetImageScaleStoryboard.Stop();
            IllustrationOriginalImageRenderTransform.ScaleX = 1;
            IllustrationOriginalImageRenderTransform.ScaleY = 1;
        }

        private void ImageScaledOut200PercentStoryboard_OnCompleted(object? sender, object e)
        {
            ImageScaledOut200PercentStoryboard.Stop();
            IllustrationOriginalImageRenderTransform.ScaleX = 2;
            IllustrationOriginalImageRenderTransform.ScaleY = 2;
        }

        private void ResetImageTranslationStoryboard_OnCompleted(object? sender, object e)
        {
            ResetImageTranslationStoryboard.Stop();
            IllustrationOriginalImageRenderTransform.TranslateX = 0;
            IllustrationOriginalImageRenderTransform.TranslateY = 0;
        }

        private void IllustrationOriginalImageContainer_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            Zoom(e.GetCurrentPoint(null).Properties.MouseWheelDelta / 500d);
        }

        #region Helper Functions

        private readonly EasingFunctionBase _easingFunction = new ExponentialEase
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 12
        };

        private bool _tappedScaled;

        private void SetSource(ImageSource imageSource)
        {
            IllustrationOriginalImage.Source = imageSource;
        }

        private double GetZoomFactor()
        {
            return IllustrationOriginalImageRenderTransform.ScaleX;
        }

        private void Zoom(double delta)
        {
            if (IllustrationOriginalImageRenderTransform.TranslateX != 0)
            {
                UIHelper.CreateStoryboard(IllustrationOriginalImageRenderTransform.CreateDoubleAnimation("TranslateX", 0, null, null, TimeSpan.FromMilliseconds(100), _easingFunction)).Begin();
            }

            if (IllustrationOriginalImageRenderTransform.TranslateY != 0)
            {
                UIHelper.CreateStoryboard(IllustrationOriginalImageRenderTransform.CreateDoubleAnimation("TranslateY", 0, null, null, TimeSpan.FromMilliseconds(100), _easingFunction)).Begin();
            }

            var factor = GetZoomFactor();
            switch (delta)
            {
                case < 0 when factor > MinZoomFactor:
                case > 0 when factor < MaxZoomFactor:
                    IllustrationOriginalImageRenderTransform.ScaleX += delta;
                    IllustrationOriginalImageRenderTransform.ScaleY += delta;
                    break;
            }
        }

        #endregion
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                    