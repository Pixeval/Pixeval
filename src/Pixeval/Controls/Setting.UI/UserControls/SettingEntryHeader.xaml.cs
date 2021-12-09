#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SettingEntryHeader.xaml.cs
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

using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Pixeval.Controls.Setting.UI.UserControls;

public sealed partial class SettingEntryHeader
{
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header),
        typeof(string),
        typeof(SettingEntryHeader),
        PropertyMetadata.Create(DependencyProperty.UnsetValue, HeaderChanged));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(object),
        typeof(SettingEntryHeader),
        PropertyMetadata.Create(DependencyProperty.UnsetValue, DescriptionChanged));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(SettingEntryHeader),
        PropertyMetadata.Create(DependencyProperty.UnsetValue, IconChanged));

    public SettingEntryHeader()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            // Prevent the button from expanding the expander
            if (DescriptionPresenter.Content is DependencyObject obj && obj.FindDescendantOrSelf<ButtonBase>() is { } buttonBase)
            {
                buttonBase.Tapped += (_, eventArgs) => eventArgs.Handled = true;
            }
        };
    }

    public string Header
    {
        get => (string) GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IconElement Icon
    {
        get => (IconElement) GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private static void HeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is string str)
        {
            ((SettingEntryHeader) d).HeaderTextBlock.Text = str;
        }
    }

    private static void DescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is { } content)
        {
            ((SettingEntryHeader) d).DescriptionPresenter.Content = content;
        }
    }

    private static void IconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var presenter = ((SettingEntryHeader) d).IconContentPresenter;
        if (e.NewValue is IconElement icon)
        {
            presenter.Visibility = Visibility.Visible;
            presenter.Content = icon;
        }
        else
        {
            presenter.Visibility = Visibility.Collapsed;
        }
    }

    private void SettingEntryHeader_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // The MaxWidth binding won't work, no idea why
        DescriptionPresenter.Width = e.NewSize.Width - IconContentPresenter.ActualWidth;
    }
}