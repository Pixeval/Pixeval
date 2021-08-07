using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
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
                var pos = IllustrationOriginalImage.TransformToVisual(IllustrationOriginalImageContainer).TransformPoint(new Point(0, 0));
                if (HorizontallyDraggable((int) pos.X))
                {
                    IllustrationOriginalImageRenderTransform.TranslateX += deltaX;
                }

                if (VerticallyDraggable((int) pos.Y))
                {
                    IllustrationOriginalImageRenderTransform.TranslateY += deltaY;
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

        private void IllustrationOriginalImage_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(null).Properties.MouseWheelDelta;
            switch (delta)
            {
                case > 0 when GetZoomFactor() < MaxZoomFactor:
                    Zoom(delta / 500d);
                    break;
                case < 0 when GetZoomFactor() > MinZoomFactor:
                    Zoom(delta / 500d);
                    break;
            }
        }

        #region Helper Functions

        private readonly EasingFunctionBase _easingFunction = new ExponentialEase
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 12
        };

        private bool _tappedScaled;

        private readonly Queue<Storyboard> _zoomAnimationQueue = new();

        private void SetSource(ImageSource imageSource)
        {
            IllustrationOriginalImage.Source = imageSource;
        }

        private double GetZoomedManipulationBound()
        {
            return 100 / GetZoomFactor();
        }

        private double GetZoomFactor()
        {
            return IllustrationOriginalImageRenderTransform.ScaleX;
        }

        private bool HorizontallyDraggable(int x)
        {
            var renderedImageWidth = IllustrationOriginalImage.ActualWidth * GetZoomFactor();
            var containerWidth = IllustrationOriginalImageContainer.ActualWidth;
            return renderedImageWidth > containerWidth && x < 0 && x > -(renderedImageWidth - containerWidth);
        }

        private bool VerticallyDraggable(int y)
        {
            var renderedImageHeight = IllustrationOriginalImage.ActualHeight * GetZoomFactor();
            var containerHeight = IllustrationOriginalImageContainer.ActualHeight;
            return renderedImageHeight > containerHeight && y < 0 && y > -(renderedImageHeight - containerHeight);
        }

        private void Zoom(double delta)
        {
            var currentScaleX = IllustrationOriginalImageRenderTransform.ScaleX;
            var currentScaleY = IllustrationOriginalImageRenderTransform.ScaleY;
            var computedScaleX = currentScaleX + delta;
            var computedScaleY = currentScaleY + delta;
            if (IllustrationOriginalImageRenderTransform.TranslateX != 0)
            {
                UIHelper.CreateStoryboard(IllustrationOriginalImageRenderTransform.CreateDoubleAnimation("TranslateX", 0, null, null, TimeSpan.FromMilliseconds(100), _easingFunction)).Begin();
            }

            if (IllustrationOriginalImageRenderTransform.TranslateY != 0)
            {
                UIHelper.CreateStoryboard(IllustrationOriginalImageRenderTransform.CreateDoubleAnimation("TranslateY", 0, null, null, TimeSpan.FromMilliseconds(100), _easingFunction)).Begin();
            }
            var sb = UIHelper.CreateStoryboard(
                IllustrationOriginalImageRenderTransform.CreateDoubleAnimation("ScaleX", computedScaleX, null, null, TimeSpan.FromMilliseconds(500), _easingFunction),
                IllustrationOriginalImageRenderTransform.CreateDoubleAnimation("ScaleY", computedScaleY, null, null, TimeSpan.FromMilliseconds(500), _easingFunction)); ;
            sb.Begin();
        }

        #endregion
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                    