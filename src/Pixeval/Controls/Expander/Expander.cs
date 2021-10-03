using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls.Card;
using Pixeval.Util.UI;

namespace Pixeval.Controls.Expander
{
    [TemplatePart(Name = PartContentCardContainer, Type = typeof(CardControl))]
    [TemplatePart(Name = PartExpanderSwitchButton, Type = typeof(IconButton))]
    [TemplatePart(Name = PartRootPanel, Type = typeof(Grid))]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Expanded")]
    public sealed class Expander : ContentControl
    {
        private const string PartContentCardContainer = "ContentCardContainer";
        private const string PartHeaderCardContainer = "HeaderCardContainer";
        private const string PartExpanderSwitchButton = "ExpandOrFoldExpanderButton";
        private const string PartRootPanel = "RootPanel";

        private CardControl? _contentCardContainer;
        private CardControl? _headerCardContainer;
        private IconButton? _expanderSwitchButton;
        private Grid? _rootPanel;

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

        public static readonly DependencyProperty NegativeHeaderHeightProperty = DependencyProperty.Register(
            nameof(NegativeHeaderHeight),
            typeof(double),
            typeof(Expander),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public double NegativeHeaderHeight
        {
            get => (double) GetValue(NegativeHeaderHeightProperty);
            set => SetValue(NegativeHeaderHeightProperty, value);
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
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        private static void HeaderHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is double value && d is Expander expander)
            {
                expander.NegativeHeaderHeight = -value;
            }
        }

        private static void IsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool value && d is Expander {_expanderSwitchButton: { } button} expander)
            {
                var size = (double) Application.Current.Resources["PixevalButtonIconSmallSize"];
                button.Icon = value
                    ? FontIconSymbols.ChevronUpSmallE96D.GetFontIcon(size)
                    : FontIconSymbols.ChevronDownSmallE96E.GetFontIcon(size);
                VisualStateManager.GoToState(expander, value ? "Expanded" : "Normal", true);
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
            _contentCardContainer = GetTemplateChild(PartContentCardContainer) as CardControl;
            NegativeHeaderHeight = -HeaderHeight;

            if (_expanderSwitchButton is not null)
            {
                _expanderSwitchButton.Tapped -= ExpandSwitchButtonOnTapped;
            }

            if ((_expanderSwitchButton = GetTemplateChild(PartExpanderSwitchButton) as IconButton) is not null)
            {
                _expanderSwitchButton.Tapped += ExpandSwitchButtonOnTapped;
            }

            if (_rootPanel is not null)
            {
                _rootPanel.PointerEntered -= RootPanelOnPointerEntered;
                _rootPanel.PointerExited -= RootPanelOnPointerExited;
            }

            if ((_rootPanel = GetTemplateChild(PartRootPanel) as Grid) is not null)
            {
                _rootPanel.PointerEntered += RootPanelOnPointerEntered;
                _rootPanel.PointerExited += RootPanelOnPointerExited;
            }

            if (_headerCardContainer is not null)
            {
                _headerCardContainer.Tapped -= HeaderCardContainerOnTapped;
            }

            if ((_headerCardContainer = GetTemplateChild(PartHeaderCardContainer) as CardControl) is not null)
            {
                _headerCardContainer.Tapped += HeaderCardContainerOnTapped;
            }

            base.OnApplyTemplate();
        }

        private void HeaderCardContainerOnTapped(object sender, TappedRoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
            e.Handled = true;
        }

        private void RootPanelOnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            _headerCardContainer!.Background = (Brush) Application.Current.Resources["ExpanderContentBackground"];
            _headerCardContainer.BorderThickness = new Thickness(1);
        }

        private void RootPanelOnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _headerCardContainer!.Background = (Brush) Application.Current.Resources["ExpanderHeaderCardPointerOverBackground"];
            _headerCardContainer.BorderThickness = new Thickness(1, 1, 1, 1.5);
        }

        private void ExpandSwitchButtonOnTapped(object sender, TappedRoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
            e.Handled = true;
        }
    }
}
