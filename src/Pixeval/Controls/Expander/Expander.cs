#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/Expander.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls.Card;
using Pixeval.Util.UI;

namespace Pixeval.Controls.Expander;

[TemplatePart(Name = PartContentCardContainer, Type = typeof(CardControl))]
[TemplatePart(Name = PartExpandOrFoldExpanderPresenter, Type = typeof(IconButton.IconButton))]
[TemplatePart(Name = PartRootPanel, Type = typeof(Grid))]
[TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
[TemplateVisualState(GroupName = "CommonStates", Name = "Expanded")]
public sealed class Expander : ContentControl
{
    private const string PartContentCardContainer = "ContentCardContainer";
    private const string PartHeaderCardContainer = "HeaderCardContainer";
    private const string PartExpandOrFoldExpanderPresenter = "ExpandOrFoldExpanderPresenter";
    private const string PartRootPanel = "RootPanel";

    public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
        nameof(HeaderHeight),
        typeof(double),
        typeof(Expander),
        PropertyMetadata.Create(DependencyProperty.UnsetValue, HeaderHeightChanged));

    public static readonly DependencyProperty NegativeHeaderHeightProperty = DependencyProperty.Register(
        nameof(NegativeHeaderHeight),
        typeof(double),
        typeof(Expander),
        PropertyMetadata.Create(DependencyProperty.UnsetValue));

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded),
        typeof(bool),
        typeof(Expander),
        PropertyMetadata.Create(false, IsExpandedChanged));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header),
        typeof(object),
        typeof(Expander),
        PropertyMetadata.Create(DependencyProperty.UnsetValue));

    private CardControl? _contentCardContainer;
    private ContentPresenter? _expandOrFoldExpanderPresenter;
    private CardControl? _headerCardContainer;
    private Grid? _rootPanel;

    public Expander()
    {
        DefaultStyleKey = typeof(Expander);
        Loaded += OnLoaded;
    }

    public double HeaderHeight
    {
        get => (double) GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    public double NegativeHeaderHeight
    {
        get => (double) GetValue(NegativeHeaderHeightProperty);
        set => SetValue(NegativeHeaderHeightProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool) GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

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
        if (e.NewValue is bool value && d is Expander { _expandOrFoldExpanderPresenter: { } presenter } expander)
        {
            presenter.Content = value
                ? FontIconSymbols.ChevronUpSmallE96D.GetFontIcon(13)
                : FontIconSymbols.ChevronDownSmallE96E.GetFontIcon(13);
            VisualStateManager.GoToState(expander, value ? "Expanded" : "Normal", true);
        }
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

        if (_expandOrFoldExpanderPresenter is not null)
        {
            _expandOrFoldExpanderPresenter.Tapped -= ExpandSwitchButtonOnTapped;
        }

        if ((_expandOrFoldExpanderPresenter = GetTemplateChild(PartExpandOrFoldExpanderPresenter) as ContentPresenter) is not null)
        {
            _expandOrFoldExpanderPresenter.Content = FontIconSymbols.ChevronDownSmallE96E.GetFontIcon(13);
            _expandOrFoldExpanderPresenter.Tapped += ExpandSwitchButtonOnTapped;
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