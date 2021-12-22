using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Animations.Expressions;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.IllustratorView;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Messages;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages.Capability
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IllustratorPage
    {
        private IllustratorViewModel _viewModel;

        private CompositionPropertySet _props;
        private CompositionPropertySet _scrollerPropertySet;
        private Compositor _compositor;
        private SpriteVisual? _blurredBackgroundImageVisual;

        public IllustrationGrid ViewModelProvider => IllustrationGrid;

        public IllustratorPage()
        {
            this.InitializeComponent();
        }

        public override void OnPageDeactivated(NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            IllustrationGrid.ViewModel.Dispose();
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        public override void OnPageActivated(NavigationEventArgs navigationEventArgs)
        {
            if (navigationEventArgs.Parameter is IllustratorViewModel viewModel)
            {
                _viewModel = viewModel;
            }
            WeakReferenceMessenger.Default.Register<IllustratorPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustrationGrid.ViewModel.FetchEngine?.Cancel());
            ChangeSource();
        }

        private void IllustratorPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.AppViewModel.Window.GetNavigationModeAndReset() is not NavigationMode.Back)
            {
                ChangeSource();
            }
            // Retrieve the ScrollViewer that the GridView is using internally
            var scrollViewer = IllustrationGrid.IllustrationGridView.FindDescendant<ScrollViewer>();

            // Update the ZIndex of the header container so that the header is above the items when scrolling
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent(Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex((UIElement)headerContainer, 1);

            // Get the PropertySet that contains the scroll values from the ScrollViewer
            _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
            _compositor = _scrollerPropertySet.Compositor;

            // Create a PropertySet that has values to be referenced in the ExpressionAnimations below
            _props = _compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);
            _props.InsertScalar("clampSize", 150);
            _props.InsertScalar("scaleFactor", 0.7f);

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
            var props = _props.GetReference();
            var progressNode = props.GetScalarProperty("progress");
            var clampSizeNode = props.GetScalarProperty("clampSize");
            var scaleFactorNode = props.GetScalarProperty("scaleFactor");

            // Create a blur effect to be animated based on scroll position
            var blurEffect = new GaussianBlurEffect()
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

            // Create a Visual for applying the blur effect
            _blurredBackgroundImageVisual = _compositor.CreateSpriteVisual();
            _blurredBackgroundImageVisual.Brush = blurBrush;
            _blurredBackgroundImageVisual.Size = new Vector2((float)OverlayRectangle.ActualWidth, (float)OverlayRectangle.ActualHeight);

            // Insert the blur visual at the right point in the Visual Tree
            ElementCompositionPreview.SetElementChildVisual(OverlayRectangle, _blurredBackgroundImageVisual);

            // Create and start an ExpressionAnimation to track scroll progress over the desired distance
            ExpressionNode progressAnimation = ExpressionFunctions.Clamp(-scrollingProperties.Translation.Y / clampSizeNode, 0, 1);
            _props.StartAnimation("progress", progressAnimation);

            // Create and start an ExpressionAnimation to animate blur radius between 0 and 15 based on progress
            ExpressionNode blurAnimation = ExpressionFunctions.Lerp(0, 15, progressNode);
            _blurredBackgroundImageVisual.Brush.Properties.StartAnimation("blur.BlurAmount", blurAnimation);

            // Get the backing visual for the header so that its properties can be animated
            var headerVisual = ElementCompositionPreview.GetElementVisual(Header);

            // Create and start an ExpressionAnimation to clamp the header's offset to keep it onscreen
            ExpressionNode headerTranslationAnimation = ExpressionFunctions.Conditional(progressNode < 1, 0, -scrollingProperties.Translation.Y - clampSizeNode);
            headerVisual.StartAnimation("Offset.Y", headerTranslationAnimation);

            // Create and start an ExpressionAnimation to scale the header during overpan
            ExpressionNode headerScaleAnimation = ExpressionFunctions.Lerp(1, 1.25f, ExpressionFunctions.Clamp(scrollingProperties.Translation.Y / 50, 0, 1));
            headerVisual.StartAnimation("Scale.X", headerScaleAnimation);
            headerVisual.StartAnimation("Scale.Y", headerScaleAnimation);

            //Set the header's CenterPoint to ensure the overpan scale looks as desired
            headerVisual.CenterPoint = new Vector3((float)(Header.ActualWidth / 2), (float)Header.ActualHeight, 0);

            // Get the backing visual for the photo in the header so that its properties can be animated
            var photoVisual = ElementCompositionPreview.GetElementVisual(BackgroundRectangle);

            // Create and start an ExpressionAnimation to opacity fade out the image behind the header
            ExpressionNode imageOpacityAnimation = 1 - progressNode;
            photoVisual.StartAnimation("opacity", imageOpacityAnimation);

            // Get the backing visual for the profile picture visual so that its properties can be animated
            var profileVisual = ElementCompositionPreview.GetElementVisual(ProfileImage);

            // Create and start an ExpressionAnimation to scale the profile image with scroll position
            ExpressionNode scaleAnimation = ExpressionFunctions.Lerp(1, scaleFactorNode, progressNode);
            profileVisual.StartAnimation("Scale.X", scaleAnimation);
            profileVisual.StartAnimation("Scale.Y", scaleAnimation);

            // Get backing visuals for the text blocks so that their properties can be animated
            var blurbVisual = ElementCompositionPreview.GetElementVisual(Blurb);
            var subtitleVisual = ElementCompositionPreview.GetElementVisual(SubtitleBlock);
            var moreVisual = ElementCompositionPreview.GetElementVisual(MoreText);

            // Create an ExpressionAnimation that moves between 1 and 0 with scroll progress, to be used for text block opacity
            ExpressionNode textOpacityAnimation = ExpressionFunctions.Clamp(1 - (progressNode * 2), 0, 1);

            // Start opacity and scale animations on the text block visuals
            blurbVisual.StartAnimation("Opacity", textOpacityAnimation);
            blurbVisual.StartAnimation("Scale.X", scaleAnimation);
            blurbVisual.StartAnimation("Scale.Y", scaleAnimation);

            subtitleVisual.StartAnimation("Opacity", textOpacityAnimation);
            subtitleVisual.StartAnimation("Scale.X", scaleAnimation);
            subtitleVisual.StartAnimation("Scale.Y", scaleAnimation);

            moreVisual.StartAnimation("Opacity", textOpacityAnimation);
            moreVisual.StartAnimation("Scale.X", scaleAnimation);
            moreVisual.StartAnimation("Scale.Y", scaleAnimation);

            // Get the backing visuals for the text and button containers so that their properites can be animated
            var textVisual = ElementCompositionPreview.GetElementVisual(TextContainer);
            var buttonVisual = ElementCompositionPreview.GetElementVisual(ButtonPanel);

            // When the header stops scrolling it is 150 pixels offscreen.  We want the text header to end up with 50 pixels of its content
            // offscreen which means it needs to go from offset 0 to 100 as we traverse through the scrollable region
            ExpressionNode contentOffsetAnimation = progressNode * 100;
            textVisual.StartAnimation("Offset.Y", contentOffsetAnimation);

            ExpressionNode buttonOffsetAnimation = progressNode * -100;
            buttonVisual.StartAnimation("Offset.Y", buttonOffsetAnimation);
        }

        private void ChangeSource()
        {
            _ = ViewModelProvider.ViewModel.VisualizationController.ResetAndFillAsync(_viewModel.FetchEngine, 100);
        }

        private void IllustratorPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_blurredBackgroundImageVisual != null)
            {
                _blurredBackgroundImageVisual.Size = new Vector2((float)OverlayRectangle.ActualWidth, (float)OverlayRectangle.ActualHeight);
            }
        }

        public void GoBack()
        {
            App.AppViewModel.AppWindowRootFrame.BackStack.RemoveAll(entry => entry.SourcePageType == typeof(IllustratorPage));
            if (App.AppViewModel.AppWindowRootFrame.CanGoBack)
            {
                App.AppViewModel.AppWindowRootFrame.GoBack(new SuppressNavigationTransitionInfo());
            }
        }

        private void BackButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            GoBack();
        }
    }
}
