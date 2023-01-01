#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/SettingEntryHeader.xaml.cs
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
using WinUI3Utilities.Attributes;


namespace Pixeval.Controls.Setting.UI.UserControls;

/// <summary>
/// The <see cref="SettingEntryHeader"/> provide use information about a setting entry, includes <see cref="Header"/>, which
/// is the name of the entry, and <see cref="Description"/>, which is a brief introduction of the entry, apart from these, it
/// also includes an <see cref="Icon"/> to give user a hint that can be immediately understand
/// </summary>
[DependencyProperty<string>("Header", nameof(OnHeaderChanged))]
[DependencyProperty<object>("Description", nameof(OnDescriptionChanged))]
[DependencyProperty<IconElement>("Icon", nameof(OnIconChanged))]
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
            ((SettingEntryHeader)d).HeaderTextBlock.Text = str;
        }
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is { } content)
        {
            ((SettingEntryHeader)d).DescriptionPresenter.Content = content;
        }
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var presenter = ((SettingEntryHeader)d).IconContentPresenter;
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
