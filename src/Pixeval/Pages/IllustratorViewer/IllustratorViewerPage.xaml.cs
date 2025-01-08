// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinUI3Utilities;
using CommunityToolkit.WinUI.Controls;
using Pixeval.Controls.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls;
using Pixeval.Util.UI;

namespace Pixeval.Pages.IllustratorViewer;

public sealed partial class IllustratorViewerPage
{
    private IllustratorViewerPageViewModel _viewModel = null!;
    private int _lastNavigationViewTag = -1;

    public IllustratorViewerPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        _viewModel = HWnd.GetIllustratorViewerPageViewModel(parameter);
    }

    private async void IllustratorViewerSegmented_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var segmented = sender.To<Segmented>();
        var currentTag = segmented.SelectedItem.To<FrameworkElement>().GetTag<NavigationViewTag?>() ?? _viewModel.WorkTag;

        IllustratorViewerFrame.NavigateTag(currentTag, new EntranceNavigationTransitionInfo());
        IllustratorViewerFrame.NavigateTag(currentTag, _lastNavigationViewTag is not -1
            ? new SlideNavigationTransitionInfo { Effect = _lastNavigationViewTag > segmented.SelectedIndex ? SlideNavigationTransitionEffect.FromLeft : SlideNavigationTransitionEffect.FromRight }
            : new EntranceNavigationTransitionInfo());
        _lastNavigationViewTag = segmented.SelectedIndex;

        _ = await IllustratorViewerFrame.AwaitPageTransitionAsync(currentTag.NavigateTo);
        StickyHeaderScrollView.RaiseSetInnerScrollView();
    }

    private double StickyHeaderScrollView_OnGetScrollableLength()
    {
        // 标题栏高度32
        return ScrollableLength.ActualHeight - 32;
    }

    private ScrollView? StickyHeaderScrollView_OnSetInnerScrollView()
    {
        return IllustratorViewerFrame.Content is IScrollViewHost { ScrollView: { } scrollView } ? scrollView : null;
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
    public Visibility IsLogoVisible()
    {
        return WindowFactory.GetWindowForElement(this).HWnd != WindowFactory.RootWindow.HWnd
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
