#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ActionableExpanderSettingEntry.cs
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


namespace Pixeval.Controls.Setting.UI.ActionableExpanderSettingEntry;

[TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
[TemplatePart(Name = PartEntryContentPresenter, Type = typeof(ContentPresenter))]
[DependencyProperty("Icon", typeof(IconElement), nameof(OnIconChanged))]
[DependencyProperty("Header", typeof(string))]
[DependencyProperty("Description", typeof(object), nameof(OnDescriptionChanged))]
[DependencyProperty("HeaderHeight", typeof(double))]
[DependencyProperty("ActionContent", typeof(object))]
[DependencyProperty("ContentMargin", typeof(Thickness))]
public partial class ActionableExpanderSettingEntry : ContentControl
{
    private const string PartEntryHeader = "EntryHeader";
    private const string PartEntryContentPresenter = "EntryContentPresenter";


    private ContentPresenter? _entryContentPresenter;

    private SettingEntryHeader? _entryHeader;

    public ActionableExpanderSettingEntry()
    {
        DefaultStyleKey = typeof(ActionableExpanderSettingEntry);
        Loaded += (_, _) => Update();
    }

    private void Update()
    {
        OnDescriptionChanged(this, Description);
        OnIconChanged(this, Icon);
    }

    protected override void OnApplyTemplate()
    {
        _entryHeader = GetTemplateChild(PartEntryHeader) as SettingEntryHeader;
        _entryContentPresenter = GetTemplateChild(PartEntryContentPresenter) as ContentPresenter;
        base.OnApplyTemplate();
    }

    private static void OnIconChanged(DependencyObject dependencyObject, object? argsNewValue)
    {
        if (dependencyObject is ActionableExpanderSettingEntry { _entryHeader: { } header, _entryContentPresenter: { } presenter })
        {
            header.Margin = argsNewValue is IconElement
                ? new Thickness(0)
                : new Thickness(10, 0, 00, 0);
            presenter.Margin = argsNewValue is IconElement
                ? new Thickness(35, 0, 35, 0)
                : new Thickness(10, 0, 10, 0);
        }
    }

    private static void OnDescriptionChanged(DependencyObject dependencyObject, object? argsNewValue)
    {
        if (dependencyObject is ActionableExpanderSettingEntry { _entryHeader: { } header })
        {
            if (argsNewValue is UIElement element)
            {
                header.Description = element;
                return;
            }

            header.Description = new TextBlock
            {
                Text = argsNewValue?.ToString() ?? string.Empty
            };
        }
    }
}