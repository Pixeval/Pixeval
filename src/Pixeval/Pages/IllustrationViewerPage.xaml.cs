using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
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

        // use int for easy comparison
        private (double, double) GetImageInViewportScaledSize()
        {
            return (IllustrationOriginalImageContainerScrollViewer.ViewportWidth / IllustrationOriginalImageContainerScrollViewer.ZoomFactor, IllustrationOriginalImageContainerScrollViewer.ViewportHeight / IllustrationOriginalImageContainerScrollViewer.ZoomFactor);
        }

        private double GetResizedManipulationBound()
        {
            return 100 / IllustrationOriginalImageContainerScrollViewer.ZoomFactor;
        }

        private void IllustrationOriginalImage_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var (scaledWidth, scaledHeight) = GetImageInViewportScaledSize();// -width - viewportScaledWith - 100 / zoomFactor
            var (horizontalBound, verticalBound) = (-IllustrationOriginalImage.ActualWidth + scaledWidth - GetResizedManipulationBound(), -IllustrationOriginalImage.ActualHeight + scaledHeight - GetResizedManipulationBound());
            var (x, y) = (IllustrationOriginalImageRenderTransform.TranslateX, IllustrationOriginalImageRenderTransform.TranslateY);
            var (translationX, translationY) = (e.Delta.Translation.X, e.Delta.Translation.Y);
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

        private void IllustrationOriginalImage_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            // TODO animation
        }
    }
}
