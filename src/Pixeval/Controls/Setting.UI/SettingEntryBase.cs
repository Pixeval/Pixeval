#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SettingEntryBase.cs
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
using Pixeval.Attributes;
using Pixeval.Controls.Setting.UI.UserControls;


namespace Pixeval.Controls.Setting.UI;

[DependencyProperty("Icon", typeof(IconElement), nameof(OnIconChanged))]
[DependencyProperty("Header", typeof(string))]
[DependencyProperty("Description", typeof(object), nameof(OnDescriptionChanged))]
public partial class SettingEntryBase : Control
{
    protected const string PartEntryHeader = "EntryHeader";

    protected SettingEntryHeader? SettingEntryHeader;

    public SettingEntryBase()
    {
        Loaded += (_, _) =>
        {
            IconChanged(this, Icon);
            DescriptionChanged(this, Description);
            Update();
        };
    }

    private static void OnIconChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        IconChanged(o, args.NewValue);
    }

    private static void OnDescriptionChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        DescriptionChanged(o, args.NewValue);
    }

    private static void IconChanged(DependencyObject dependencyObject, object argsNewValue)
    {
        if (dependencyObject is SettingEntryBase entry)
        {
            entry.IconChanged(argsNewValue);
        }
    }

    protected static void DescriptionChanged(DependencyObject dependencyObject, object? argsNewValue)
    {
        if (dependencyObject is SettingEntryBase entry)
        {
            entry.DescriptionChanged(argsNewValue);
        }
    }

    protected virtual void IconChanged(object? newValue)
    {
        if (SettingEntryHeader is not null)
        {
            SettingEntryHeader.Margin = newValue is IconElement
                ? new Thickness(0)
                : new Thickness(10, 0, 10, 0);
        }
    }

    public virtual void DescriptionChanged(object? newValue)
    {
        if (SettingEntryHeader is not null)
        {
            if (newValue is UIElement element)
            {
                SettingEntryHeader.Description = element;
                return;
            }

            SettingEntryHeader.Description = new TextBlock
            {
                Text = newValue?.ToString() ?? string.Empty
            };
        }
    }

    protected virtual void Update()
    {
    }

    protected override void OnApplyTemplate()
    {
        SettingEntryHeader = (SettingEntryHeader?) GetTemplateChild(PartEntryHeader);
        base.OnApplyTemplate();
    }
}