using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.Card;
using Pixeval.Util.UI;

namespace Pixeval.Controls.Expander
{
    public sealed class Expander : ContentControl
    {
        private ContentPresenter? _headerPresenter;
        private CardControl? _headerCardContainer;
        private CardControl? _contentCardContainer;
        private IconButton? _expandSwitchButton;

        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
            nameof(HeaderHeight),
            typeof(double),
            typeof(Expander),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, HeaderHeightChanged));

        public double HeaderHeight
        {
            get => (double) GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            nameof(IsExpanded), 
            typeof(bool), 
            typeof(Expander), 
            PropertyMetadata.Create(false, IsExpandedChanged));

        public bool IsExpanded
        {
            get => (bool) GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), 
            typeof(object), 
            typeof(Expander), 
            PropertyMetadata.Create(DependencyProperty.UnsetValue, HeaderChanged));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        private static void HeaderHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is double value && d is Expander {_headerPresenter: { } presenter, _headerCardContainer: { } container})
            {
                presenter.Height = value;
                container.Height = value;
            }
        }

        private static void IsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool value && d is Expander {_expandSwitchButton: { } button} expander)
            {
                var size = (double) Application.Current.Resources["PixevalButtonIconSmallSize"];
                button.Icon = value
                    ? FontIconSymbols.ChevronUpSmallE96D.GetFontIcon(size)
                    : FontIconSymbols.ChevronDownSmallE96E.GetFontIcon(size);
                VisualStateManager.GoToState(expander, value ? "Expanded" : "Normal", true);
                var easingFunction = new ExponentialEase {Exponent = 12};
                if (value)
                {
                    expander._contentCardContainer!.CreateDoubleAnimation("(UIElement.RenderTransform).(CompositeTransform.TranslateY)", from: -expander.HeaderHeight, to: 0, duration: TimeSpan.FromMilliseconds(333), easingFunction: easingFunction)
                        .BeginStoryboard();
                    expander._contentCardContainer!.CreateDoubleAnimation("Opacity", from: 0, to: 1, duration: TimeSpan.Zero)
                        .BeginStoryboard();
                }
                else
                {
                    expander._contentCardContainer!.CreateDoubleAnimation("(UIElement.RenderTransform).(CompositeTransform.TranslateY)", from: 0, to: -expander.HeaderHeight, duration: TimeSpan.FromMilliseconds(200), easingFunction: easingFunction)
                        .BeginStoryboard();
                    expander._contentCardContainer!.CreateDoubleAnimation("Opacity", from: 1, to: 0, duration: TimeSpan.FromMilliseconds(200), easingFunction: easingFunction)
                        .BeginStoryboard();
                }
            }
        }

        private static void HeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is { } content && d is Expander {_headerPresenter: { } presenter})
            {
                presenter.Content = content;
            }
        }

        public Expander()
        {
            DefaultStyleKey = typeof(Expander);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _contentCardContainer!.Margin = new Thickness(0, HeaderHeight, 0, 0);
            VisualStateManager.GoToState(this, "Normal", true);
        }

        protected override void OnApplyTemplate()
        {
            _headerPresenter = (ContentPresenter) GetTemplateChild("HeaderPresenter");
            _expandSwitchButton = (IconButton) GetTemplateChild("ExpandOrFoldExpanderButton");
            _contentCardContainer = (CardControl) GetTemplateChild("ContentCardContainer");
            _headerCardContainer = (CardControl) GetTemplateChild("HeaderCardContainer");
            _headerPresenter.Content = Header;
            _headerPresenter.Height = HeaderHeight;
            _expandSwitchButton.Tapped += (_, _) => IsExpanded = !IsExpanded;
            base.OnApplyTemplate();
        }
    }
}
