using System;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public sealed partial class ImageViewerPage
    {
        private ImageViewerPageViewModel _viewModel = null!;

        public ImageViewerPage()
        {
            InitializeComponent();
        }

        public override void OnPageActivated(NavigationEventArgs e)
        {
            _viewModel = (ImageViewerPageViewModel) e.Parameter;
        }

        private void IllustrationOriginalImage_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var (deltaX, deltaY) = (e.Delta.Translation.X, e.Delta.Translation.Y);
            if (GetZoomFactor() > 1)
            {
                const int overflowPermittedInPixel = 100;
                var renderedImageWidth = IllustrationOriginalImage.ActualWidth * GetZoomFactor();
                var renderedImageHeight = IllustrationOriginalImage.ActualHeight * GetZoomFactor();
                var containerWidth = IllustrationOriginalImageContainer.ActualWidth;
                var containerHeight = IllustrationOriginalImageContainer.ActualHeight;
                var imagePos = IllustrationOriginalImage.TransformToVisual(IllustrationOriginalImageContainer).TransformPoint(new Point(0, 0));
                if (renderedImageWidth > containerWidth)
                {
                    switch (deltaX)
                    {
                        case < 0 when imagePos.X > -(renderedImageWidth - containerWidth) - overflowPermittedInPixel:
                        case > 0 when imagePos.X < overflowPermittedInPixel:
                            IllustrationOriginalImageRenderTransform.TranslateX += deltaX;
                            break;
                    }
                }

                if (renderedImageHeight > containerHeight)
                {
                    switch (deltaY)
                    {
                        case < 0 when imagePos.Y > -(renderedImageHeight - containerHeight) - overflowPermittedInPixel:
                        case > 0 when imagePos.Y < overflowPermittedInPixel:
                            IllustrationOriginalImageRenderTransform.TranslateY += deltaY;
                            break;
                    }
                }
            }
        }

        private void IllustrationOriginalImage_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var renderedImageWidth = IllustrationOriginalImage.ActualWidth * GetZoomFactor();
            var renderedImageHeight = IllustrationOriginalImage.ActualHeight * GetZoomFactor();
            var containerWidth = IllustrationOriginalImageContainer.ActualWidth;
            var containerHeight = IllustrationOriginalImageContainer.ActualHeight;
            var pos = IllustrationOriginalImage.TransformToVisual(IllustrationOriginalImageContainer).TransformPoint(new Point(0, 0));

            var sb = new Storyboard();
            var animationDuration = TimeSpan.FromMilliseconds(200);
            const string translateXProperty = nameof(CompositeTransform.TranslateX);
            const string translateYProperty = nameof(CompositeTransform.TranslateY);
            var xLowerBound = -(renderedImageWidth - containerWidth);
            var yLowerBound = -(renderedImageHeight - containerHeight);

            switch (pos.X)
            {
                case var x when x < xLowerBound && renderedImageWidth > containerWidth:
                    sb.Children.Add(IllustrationOriginalImageRenderTransform.CreateDoubleAnimation(translateXProperty, animationDuration, by: xLowerBound - x));
                    break;
                case > 0 when renderedImageWidth > containerWidth:
                    sb.Children.Add(IllustrationOriginalImageRenderTransform.CreateDoubleAnimation(translateXProperty, animationDuration, by: -pos.X));
                    break;
            }

            switch (pos.Y)
            {
                case var y when y < yLowerBound && renderedImageHeight > containerHeight:
                    sb.Children.Add(IllustrationOriginalImageRenderTransform.CreateDoubleAnimation(translateYProperty, animationDuration, by: yLowerBound - y));
                    break;
                case > 0 when renderedImageHeight > containerHeight:
                    sb.Children.Add(IllustrationOriginalImageRenderTransform.CreateDoubleAnimation(translateYProperty, animationDuration, by: -pos.Y));
                    break;
            }

            if (sb.Children.Any())
            {
                sb.Begin();
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
                ImageScaledIn200PercentStoryboard.Begin();
                _tappedScaled.Inverse();
            }
        }

        private void ResetImageScaleStoryboard_OnCompleted(object? sender, object e)
        {
            ResetImageScaleStoryboard.Stop();
            IllustrationOriginalImageRenderTransform.ScaleX = 1;
            IllustrationOriginalImageRenderTransform.ScaleY = 1;
        }

        private void ImageScaledIn200PercentStoryboard_OnCompleted(object? sender, object e)
        {
            ImageScaledIn200PercentStoryboard.Stop();
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

        private void ImageViewerPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            CommandBorderDropShadow.Receivers.Add(IllustrationOriginalImageContainer);
        }

        private void IllustrationInfoAndCommentsMenuFlyoutItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.IllustrationViewerPageViewModel.IsInfoPaneOpen = true;
        }

        #region Helper Functions

        private readonly EasingFunctionBase _easingFunction = new ExponentialEase
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 12
        };

        private bool _tappedScaled;

        private double GetZoomFactor()
        {
            return IllustrationOriginalImageRenderTransform.ScaleX;
        }

        private void ResetImagePosition()
        {
            if (IllustrationOriginalImageRenderTransform.TranslateX != 0)
            {
                IllustrationOriginalImageRenderTransform.CreateDoubleAnimation(
                    nameof(CompositeTransform.TranslateX),
                    to: 0,
                    duration: TimeSpan.FromMilliseconds(100),
                    easingFunction: _easingFunction).BeginStoryboard();
            }

            if (IllustrationOriginalImageRenderTransform.TranslateY != 0)
            {
                IllustrationOriginalImageRenderTransform.CreateDoubleAnimation(
                    nameof(CompositeTransform.TranslateY),
                    to: 0,
                    duration: TimeSpan.FromMilliseconds(100),
                    easingFunction: _easingFunction).BeginStoryboard();
            }
        }

        private void Zoom(double delta)
        {
            ResetImagePosition();
            _viewModel.Zoom(delta);
        }

        #endregion
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                               