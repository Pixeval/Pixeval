using System;
using System.Threading.Tasks;
using ABI.Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.UserControls;
using Pixeval.Util;

namespace Pixeval.Pages
{
    public sealed partial class IllustrationViewerPage
    {
        public IllustrationViewerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ConnectedAnimationService.GetForCurrentView().GetAnimation(IllustrationGrid.ConnectedAnimationKey) is { } animation)
            {
                animation.TryStart(IllustrationOriginalImage);
                animation.TryStart(IllustrationOriginalImage);
            }
        }

        private void IllustrationOriginalImage_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var (scaledWidth, scaledHeight) = GetImageInViewportScaledSize();
            var (horizontalBound, verticalBound) = (-IllustrationOriginalImage.ActualWidth + scaledWidth - GetResizedManipulationBound(), -IllustrationOriginalImage.ActualHeight + scaledHeight - GetResizedManipulationBound());
            var (x, y) = (IllustrationOriginalImageRenderTransform.TranslateX, IllustrationOriginalImageRenderTransform.TranslateY);
            var (translationX, translationY) = (e.Delta.Translation.X, e.Delta.Translation.Y);
            if (WidthOverflow() && IllustrationOriginalImageContainerScrollViewer.ZoomFactor > 1)
            {
                switch (translationX)
                {
                    case < 0:
                        if ((int) x > (int) horizontalBound)
                        {
                            IllustrationOriginalImageRenderTransform.TranslateX += translationX;
                        }

                        break;
                    case >= 0:
                        if ((int) x < (int) GetResizedManipulationBound())
                        {
                            IllustrationOriginalImageRenderTransform.TranslateX += translationX;
                        }

                        break;
                }
            }

            if (HeightOverflow() && IllustrationOriginalImageContainerScrollViewer.ZoomFactor > 1)
            {
                switch (translationY)
                {
                    case < 0:
                        if ((int) y > (int) verticalBound)
                        {
                            IllustrationOriginalImageRenderTransform.TranslateY += translationY;
                        }

                        break;
                    case >= 0:
                        if ((int) y < (int) GetResizedManipulationBound())
                        {
                            IllustrationOriginalImageRenderTransform.TranslateY += translationY;
                        }

                        break;
                }
            }
        }

        private void IllustrationOriginalImage_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var manipulationBound = (int) GetResizedManipulationBound();
            var (scaledWidth, scaledHeight) = ((int, int)) GetImageInViewportScaledSize();
            var (horizontalBound, verticalBound) = ((int, int)) (-IllustrationOriginalImage.ActualWidth + scaledWidth - GetResizedManipulationBound(), -IllustrationOriginalImage.ActualHeight + scaledHeight - GetResizedManipulationBound());
            var (x, y) = ((int, int)) (IllustrationOriginalImageRenderTransform.TranslateX, IllustrationOriginalImageRenderTransform.TranslateY);
            if ((x <= horizontalBound || x >= manipulationBound || y <= verticalBound || y >= manipulationBound) && IllustrationOriginalImageContainerScrollViewer.ZoomFactor > 1)
            {
                TranslateImage(-CalculateMediumWidth(), -CalculateMediumHeight());
            }
        }

        #region Helper Functions

        private readonly EasingFunctionBase _easingFunction = new CubicEase();

        private bool _tappedScaled;

        private double CalculateMediumWidth()
        {
            return IllustrationOriginalImage.ActualWidth / 4;
        }

        private double CalculateMediumHeight()
        {
            return IllustrationOriginalImage.ActualHeight / 4;
        }

        private bool WidthOverflow()
        {
            return IllustrationOriginalImage.ActualWidth * IllustrationOriginalImageContainerScrollViewer.ZoomFactor > IllustrationOriginalImageContainerScrollViewer.ViewportWidth;
        }

        private bool HeightOverflow()
        {
            return IllustrationOriginalImage.ActualHeight * IllustrationOriginalImageContainerScrollViewer.ZoomFactor > IllustrationOriginalImageContainerScrollViewer.ViewportHeight;
        }

        // use int for easy comparison
        private (double, double) GetImageInViewportScaledSize()
        {
            return (IllustrationOriginalImageContainerScrollViewer.ViewportWidth / IllustrationOriginalImageContainerScrollViewer.ZoomFactor, IllustrationOriginalImageContainerScrollViewer.ViewportHeight / IllustrationOriginalImageContainerScrollViewer.ZoomFactor);
        }

        private double GetResizedManipulationBound()
        {
            return 100 / IllustrationOriginalImageContainerScrollViewer.ZoomFactor;
        }

        private Task TranslateImage(double toX, double toY)
        {
            var storyboard = new Storyboard();
            var xAnimation = IllustrationOriginalImageRenderTransform.CreateDoubleAnimation("TranslateX", toX, null, null, TimeSpan.FromMilliseconds(100), _easingFunction);
            var yAnimation = IllustrationOriginalImageRenderTransform.CreateDoubleAnimation("TranslateY", toY, null, null, TimeSpan.FromMilliseconds(100), _easingFunction);
            storyboard.Children.Add(xAnimation);
            storyboard.Children.Add(yAnimation);
            storyboard.Completed += (_, _) =>
            {
                storyboard.Stop();
                IllustrationOriginalImageRenderTransform.TranslateX = toX;
                IllustrationOriginalImageRenderTransform.TranslateY = toY;
            };
            return storyboard.BeginAsync();
        }

        #endregion

        private async void IllustrationOriginalImage_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_tappedScaled)
            {
                await TranslateImage(0, 0);
            }
            IllustrationOriginalImageContainerScrollViewer.ChangeView(0, 0, _tappedScaled ? 1 : 2, false);
            IllustrationOriginalImage.UpdateLayout();
            _tappedScaled.Inverse();
        }

        private void ResetImageScaleStoryboard_OnCompleted(object? sender, object e)
        {
            ResetImageScaleStoryboard.Stop();
        }
    }
}
