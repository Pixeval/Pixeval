#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/ImageViewerPage.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class ImageViewerPage
{
    private ImageViewerPageViewModel _viewModel = null!;

    private readonly AsyncLatch _resetImagePositionAnimation;

    public ImageViewerPage()
    {
        InitializeComponent();
        _resetImagePositionAnimation = new AsyncLatch(() =>
        {
            var translateXAnimation = IllustrationOriginalImageRenderTransform.CreateDoubleAnimation(
                nameof(CompositeTransform.TranslateX),
                from: IllustrationOriginalImageRenderTransform.TranslateX,
                to: 0,
                duration: TimeSpan.FromSeconds(1),
                easingFunction: _easingFunction);
            var translateYAnimation = IllustrationOriginalImageRenderTransform.CreateDoubleAnimation(
                nameof(CompositeTransform.TranslateY),
                from: IllustrationOriginalImageRenderTransform.TranslateY,
                to: 0,
                duration: TimeSpan.FromSeconds(1),
                easingFunction: _easingFunction);
            UIHelper.CreateStoryboard(translateXAnimation, translateYAnimation).Begin();
            return Task.CompletedTask;
        });
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is ImageViewerPageViewModel viewModel)
        {
            _viewModel = viewModel;
        }
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
            var imagePos = IllustrationOriginalImage.TransformToVisual(IllustrationOriginalImageContainer).TransformPoint(new Point(0, 0));
            if (renderedImageWidth > containerWidth)
            {
                switch (deltaX)
                {
                    case < 0 when imagePos.X > -(renderedImageWidth - containerWidth):
                    case > 0 when imagePos.X < 0:
                        IllustrationOriginalImageRenderTransform.TranslateX += deltaX;
                        break;
                }
            }

            if (renderedImageHeight > containerHeight)
            {
                switch (deltaY)
                {
                    case < 0 when imagePos.Y > -(renderedImageHeight - containerHeight):
                    case > 0 when imagePos.Y < 0:
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
            Zoom(1 - GetZoomFactor());
        }
        else
        {
            var illustWidth = _viewModel.IllustrationViewModel.Illustration.Width;
            var illustHeight = _viewModel.IllustrationViewModel.Illustration.Height;
            var displayImageResolution = UIHelper.GetImageScaledFactor(
                illustWidth,
                illustHeight,
                IllustrationOriginalImage.ActualWidth,
                IllustrationOriginalImage.ActualHeight, 
                GetZoomFactor());
            Zoom(displayImageResolution >= 1 ? displayImageResolution * 2 : 1 / displayImageResolution - GetZoomFactor());
        }
    }

    private void IllustrationOriginalImageContainer_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    { 
        Zoom(e.GetCurrentPoint(null).Properties.MouseWheelDelta / 1000d);
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
    
    private double GetZoomFactor()
    {
        return IllustrationOriginalImageRenderTransform.ScaleX;
    }

    private void Zoom(double delta)
    {
        _resetImagePositionAnimation.RunAsync().Discard();
        _viewModel.Zoom(delta);
    }

    #endregion
}