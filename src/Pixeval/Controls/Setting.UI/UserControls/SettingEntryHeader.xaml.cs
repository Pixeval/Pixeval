﻿#region Copyright (c) Pixeval/Pixeval

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
using Pixeval.Attributes;


namespace Pixeval.Controls.Setting.UI.UserControls;

[DependencyProperty("Header", typeof(string), nameof(OnHeaderChanged))]
[DependencyProperty("Description", typeof(object), nameof(OnDescriptionChanged))]
[DependencyProperty("Icon", typeof(IconElement), nameof(OnIconChanged))]
public sealed partial class SettingEntryHeader
{
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

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is string str)
        {
            ((SettingEntryHeader) d).HeaderTextBlock.Text = str;
        }
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is { } content)
        {
            ((SettingEntryHeader) d).DescriptionPresenter.Content = content;
        }
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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