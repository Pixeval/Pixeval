#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustratorPage.xaml.cs
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
using System.Numerics;
using Windows.System;
using CommunityToolkit.WinUI.UI.Animations.Expressions;
using Microsoft.Graphics.Canvas.Effects;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls.IllustratorView;
using Pixeval.Messages;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.UI;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustratorViewer;

public sealed partial class IllustratorPage
{
    private IllustratorViewModel? _viewModel;

    private CompositionPropertySet? _props;
    private CompositionPropertySet? _scrollerPropertySet;
    private Compositor? _compositor;
    private SpriteVisual? _blurredBackgroundImageVisual;

    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public IllustratorPage()
    {
        InitializeComponent();
    }

    public XamlUICommand OpenLinkCommand { get; } = new()
    {
        Label = IllustratorPageResources.OpenLink,
        IconSource = FontIconSymbols.LinkE71B.GetFontIconSource()
    };

    public XamlUICommand FollowCommand { get; } = new()
    {
        Label = IllustratorPageResources.Follow,
        IconSource = FontIconSymbols.HeartEB51.GetFontIconSource()
    };

    public XamlUICommand UnfollowCommand { get; } = new()
    {
        Label = IllustratorPageResources.Unfollow,
        IconSource = FontIconSymbols.HeartFillEB52.GetFontIconSource(foregroundBrush: new SolidColorBrush(Colors.Crimson))
    };

    public XamlUICommand PrivateFollowCommand { get; } = new()
    {
        Label = IllustratorPageResources.PrivateFollow,
        IconSource = FontIconSymbols.HeartEB51.GetFontIconSource()
    };

    public override void OnPageDeactivated(NavigatingCancelEventArgs navigatingCancelEventArgs)
    {
        ViewModelProvider.ViewModel.Dispose();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public override void OnPageActivated(NavigationEventArgs navigationEventArgs)
    {
        switch (navigationEventArgs.Parameter)
        {
            case (UIElement sender, IllustratorViewModel viewModel):
                WeakReferenceMessenger.Default.Send(new MainPageFrameSetConnectedAnimationTargetMessage(sender));
                ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation")?.TryStart(ProfileImage);
                _viewModel = viewModel;
                break;
            case IllustratorViewModel viewModel1:
                _viewModel = viewModel1;
                break;
        }

        WeakReferenceMessenger.Default.TryRegister<IllustratorPage, MainPageFrameNavigatingEvent>(this, static (recipient, _) => recipient.ViewModelProvider.ViewModel.DataProvider.FetchEngine?.Cancel());

        ChangeSource();
    }

    private void IllustratorPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Retrieve the ScrollViewer that the GridView is using internally
        var headerPresenter = (UIElement)VisualTreeHelper.GetParent(Header);
        var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
        Canvas.SetZIndex(headerContainer, 1);

        _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(IllustrationContainer.IllustrationView.ScrollViewer);
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

        ExpressionNode progressAnimation = ExpressionFunctions.Clamp(-scrollingProperties.Translation.Y / clampSizeNode, 0, 1);
        _props.StartAnimation("progress", progressAnimation);

        ExpressionNode blurAnimation = ExpressionFunctions.Lerp(0, 15, progressNode);
        _blurredBackgroundImageVisual.Brush.Properties.StartAnimation("blur.BlurAmount", blurAnimation);

        var headerVisual = ElementCompositionPreview.GetElementVisual(Header);

        ExpressionNode headerTranslationAnimation = ExpressionFunctions.Conditional(progressNode < 1, 0, -scrollingProperties.Translation.Y - clampSizeNode);
        headerVisual.StartAnimation("Offset.Y", headerTranslationAnimation);

        ExpressionNode headerScaleAnimation = ExpressionFunctions.Lerp(1, 1.25f, ExpressionFunctions.Clamp(scrollingProperties.Translation.Y / 50, 0, 1));
        headerVisual.StartAnimation("Scale.X", headerScaleAnimation);
        headerVisual.StartAnimation("Scale.Y", headerScaleAnimation);

        headerVisual.CenterPoint = new Vector3((float)(Header.ActualWidth / 2), (float)Header.ActualHeight, 0);

        var photoVisual = ElementCompositionPreview.GetElementVisual(BackgroundRectangle);

        ExpressionNode imageOpacityAnimation = 1 - progressNode;
        photoVisual.StartAnimation("opacity", imageOpacityAnimation);

        var profileVisual = ElementCompositionPreview.GetElementVisual(ProfileImage);

        ExpressionNode scaleAnimation = ExpressionFunctions.Lerp(1, scaleFactorNode, progressNode);
        profileVisual.StartAnimation("Scale.X", scaleAnimation);
        profileVisual.StartAnimation("Scale.Y", scaleAnimation);

        var blurbVisual = ElementCompositionPreview.GetElementVisual(Blurb);
        var subtitleVisual = ElementCompositionPreview.GetElementVisual(SubtitleBlock);

        ExpressionNode textOpacityAnimation = ExpressionFunctions.Clamp(1 - (progressNode * 2), 0, 1);

        // Start opacity and scale animations on the text block visuals
        blurbVisual.StartAnimation("Opacity", textOpacityAnimation);
        blurbVisual.StartAnimation("Scale.X", scaleAnimation);
        blurbVisual.StartAnimation("Scale.Y", scaleAnimation);

        subtitleVisual.StartAnimation("Opacity", textOpacityAnimation);
        subtitleVisual.StartAnimation("Scale.X", scaleAnimation);
        subtitleVisual.StartAnimation("Scale.Y", scaleAnimation);

        var textVisual = ElementCompositionPreview.GetElementVisual(TextContainer);
        var buttonVisual = ElementCompositionPreview.GetElementVisual(CommandBar);

        ExpressionNode contentOffsetAnimation = progressNode * 100;
        textVisual.StartAnimation("Offset.Y", contentOffsetAnimation);

        ExpressionNode buttonOffsetAnimation = progressNode * -100;
        buttonVisual.StartAnimation("Offset.Y", buttonOffsetAnimation);
    }

    private void ChangeSource()
    {
        // _ = ViewModelProvider.ViewModel.ResetEngineAndFillAsync(_viewModel!.FetchEngine, 100);
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
        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", ProfileImage);
        Parent.To<Frame>().GoBack(new SuppressNavigationTransitionInfo());
    }

    private void BackButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        GoBack();
    }

    private async void OpenLinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri($"https://www.pixiv.net/users/{_viewModel!.Id}"));
    }

    private async void FollowButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await _viewModel!.Follow();
    }

    private async void PrivateFollowButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await _viewModel!.PrivateFollow();
    }

    private async void UnfollowButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await _viewModel!.Unfollow();
    }
}
