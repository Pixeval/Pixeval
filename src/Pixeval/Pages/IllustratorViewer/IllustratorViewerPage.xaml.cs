#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorPage.xaml.cs
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

using Windows.Graphics;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinUI3Utilities;
using System.Collections.Generic;
using System;
using System.Linq;
using CommunityToolkit.WinUI.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Utilities;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Controls;

namespace Pixeval.Pages.IllustratorViewer;

public sealed partial class IllustratorViewerPage
{
    private readonly ISet<Page> _pageCache;
    private IllustratorViewerPageViewModel _viewModel = null!;
    private int _lastNavigationViewTag = -1;

    public IllustratorViewerPage()
    {
        InitializeComponent();
        _pageCache = new HashSet<Page>();
    }

    public void Dispose()
    {
        _pageCache.OfType<IDisposable>().ForEach(p => p.Dispose());
        _pageCache.Clear();
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        _viewModel = Window.Content.To<FrameworkElement>().GetViewModel(parameter);
    }

    protected override void SetTitleBarDragRegion(InputNonClientPointerSource sender, SizeInt32 windowSize, double scaleFactor, out int titleBarHeight)
    {
        sender.SetRegionRects(NonClientRegionKind.Icon, [GetScaledRect(Icon)]);
        titleBarHeight = 32;
    }

    private void GenerateLinkToThisPageButtonTeachingTip_OnActionButtonClick(TeachingTip sender, object e)
    {
        GenerateLinkTeachingTip.IsOpen = false;
        App.AppViewModel.AppSettings.DisplayTeachingTipWhenGeneratingAppLink = false;
    }

    private async void IllustratorViewerSegmented_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var segmented = sender.To<Segmented>();
        var currentTag = segmented.SelectedItem.GetTag<NavigationViewTag?>() ?? _viewModel.IllustrationTag;

        IllustratorViewerFrame.NavigateTag(currentTag, new EntranceNavigationTransitionInfo());
        IllustratorViewerFrame.NavigateTag(currentTag, _lastNavigationViewTag is not -1
            ? new SlideNavigationTransitionInfo { Effect = _lastNavigationViewTag > segmented.SelectedIndex ? SlideNavigationTransitionEffect.FromLeft : SlideNavigationTransitionEffect.FromRight }
            : new EntranceNavigationTransitionInfo());
        _lastNavigationViewTag = segmented.SelectedIndex;

        var page = await IllustratorViewerFrame.AwaitPageTransitionAsync(currentTag.NavigateTo);
        _ = _pageCache.Add(page);
        StickyHeaderScrollView.RaiseSetInnerScrollView();
    }

    private double StickyHeaderScrollView_OnGetScrollableLength()
    {
        // 标题栏高度32
        return ScrollableLength.ActualHeight - 32;
    }

    private ScrollView? StickyHeaderScrollView_OnSetInnerScrollView()
    {
        return IllustratorViewerFrame.Content is IllustratorContentViewerSubPage { ScrollView: { } scrollView } ? scrollView : null;
    }

#if false // TODO 这是什么
    private CompositionPropertySet? _props;
    private CompositionPropertySet? _scrollerPropertySet;
    private SpriteVisual? _blurredBackgroundImageVisual;
    private Compositor? _compositor;

    private void IllustratorPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Retrieve the ScrollViewer that the GridView is using internally
        var headerPresenter = (UIElement)VisualTreeHelper.GetParent(Header);
        var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
        Canvas.SetZIndex(headerContainer, 1);
        // _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(IllustrationContainer.IllustrationView.ScrollView);
        _compositor = _scrollerPropertySet.Compositor;

        _props = _compositor.CreatePropertySet();
        _props.InsertScalar("progress", 0);
        _props.InsertScalar("clampSize", 150);
        _props.InsertScalar("scaleFactor", 0.7f);

        var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
        var props = _props.GetReference();
        var progressNode = props.GetScalarProperty("progress");
        var clampSizeNode = props.GetScalarProperty("clampSize");
        var scaleFactorNode = props.GetScalarProperty("scaleFactor");

        var blurEffect = new GaussianBlurEffect
        {
            Name = "blur",
            BlurAmount = 0.0f,
            BorderMode = EffectBorderMode.Hard,
            Optimization = EffectOptimization.Balanced,
            Source = new CompositionEffectSourceParameter("source")
        };

        var blurBrush = _compositor.CreateEffectFactory(
                blurEffect,
                new[] { "blur.BlurAmount" })
            .CreateBrush();

        blurBrush.SetSourceParameter("source", _compositor.CreateBackdropBrush());

        _blurredBackgroundImageVisual = _compositor.CreateSpriteVisual();
        _blurredBackgroundImageVisual.Brush = blurBrush;
        _blurredBackgroundImageVisual.Size = new Vector2((float)OverlayRectangle.ActualWidth, (float)OverlayRectangle.ActualHeight);

        ElementCompositionPreview.SetElementChildVisual(OverlayRectangle, _blurredBackgroundImageVisual);

        var progressAnimation = ExpressionFunctions.Clamp(-scrollingProperties.Translation.Y / clampSizeNode, 0, 1);
        _props.StartAnimation("progress", progressAnimation);

        var blurAnimation = ExpressionFunctions.Lerp(0, 15, progressNode);
        _blurredBackgroundImageVisual.Brush.Properties.StartAnimation("blur.BlurAmount", blurAnimation);

        var headerVisual = ElementCompositionPreview.GetElementVisual(Header);

        var headerTranslationAnimation = ExpressionFunctions.Conditional(progressNode < 1, 0, -scrollingProperties.Translation.Y - clampSizeNode);
        headerVisual.StartAnimation("Offset.Y", headerTranslationAnimation);

        var headerScaleAnimation = ExpressionFunctions.Lerp(1, 1.25f, ExpressionFunctions.Clamp(scrollingProperties.Translation.Y / 50, 0, 1));
        headerVisual.StartAnimation("Scale.X", headerScaleAnimation);
        headerVisual.StartAnimation("Scale.Y", headerScaleAnimation);

        headerVisual.CenterPoint = new Vector3((float)(Header.ActualWidth / 2), (float)Header.ActualHeight, 0);

        var photoVisual = ElementCompositionPreview.GetElementVisual(BackgroundRectangle);

        var imageOpacityAnimation = 1 - progressNode;
        photoVisual.StartAnimation("opacity", imageOpacityAnimation);

        var profileVisual = ElementCompositionPreview.GetElementVisual(ProfileImage);

        var scaleAnimation = ExpressionFunctions.Lerp(1, scaleFactorNode, progressNode);
        profileVisual.StartAnimation("Scale.X", scaleAnimation);
        profileVisual.StartAnimation("Scale.Y", scaleAnimation);

        var blurbVisual = ElementCompositionPreview.GetElementVisual(Blurb);
        var subtitleVisual = ElementCompositionPreview.GetElementVisual(SubtitleBlock);

        var textOpacityAnimation = ExpressionFunctions.Clamp(1 - (progressNode * 2), 0, 1);

        // Start opacity and scale animations on the text block visuals
        blurbVisual.StartAnimation("Opacity", textOpacityAnimation);
        blurbVisual.StartAnimation("Scale.X", scaleAnimation);
        blurbVisual.StartAnimation("Scale.Y", scaleAnimation);

        subtitleVisual.StartAnimation("Opacity", textOpacityAnimation);
        subtitleVisual.StartAnimation("Scale.X", scaleAnimation);
        subtitleVisual.StartAnimation("Scale.Y", scaleAnimation);

        var textVisual = ElementCompositionPreview.GetElementVisual(TextContainer);
        var buttonVisual = ElementCompositionPreview.GetElementVisual(CommandBar);

        var contentOffsetAnimation = progressNode * 100;
        textVisual.StartAnimation("Offset.Y", contentOffsetAnimation);

        var buttonOffsetAnimation = progressNode * -100;
        buttonVisual.StartAnimation("Offset.Y", buttonOffsetAnimation);
    }

    private void IllustratorPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_blurredBackgroundImageVisual != null)
        {
            _blurredBackgroundImageVisual.Size = new Vector2((float)OverlayRectangle.ActualWidth, (float)OverlayRectangle.ActualHeight);
        }
    }
#endif
}
