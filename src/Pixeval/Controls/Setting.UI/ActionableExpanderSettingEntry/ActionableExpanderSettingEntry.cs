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
using Pixeval.Controls.Setting.UI.UserControls;

namespace Pixeval.Controls.Setting.UI.ActionableExpanderSettingEntry
{
    [TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
    [TemplatePart(Name = PartEntryContentPresenter, Type = typeof(ContentPresenter))]
    public class ActionableExpanderSettingEntry : ContentControl
    {
        private const string PartEntryHeader = "EntryHeader";
        private const string PartEntryContentPresenter = "EntryContentPresenter";

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(ActionableExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) => IconChanged(o, args.NewValue)));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(ActionableExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description),
            typeof(object),
            typeof(ActionableExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) => DescriptionChanged(o, args.NewValue)));

        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
            nameof(HeaderHeight),
            typeof(double),
            typeof(ActionableExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));


        public static readonly DependencyProperty ActionContentProperty = DependencyProperty.Register(
            nameof(ActionContent),
            typeof(object),
            typeof(ActionableExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(
            nameof(ContentMargin),
            typeof(Thickness),
            typeof(ActionableExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        private ContentPresenter? _entryContentPresenter;

        private SettingEntryHeader? _entryHeader;

        public ActionableExpanderSettingEntry()
        {
            DefaultStyleKey = typeof(ActionableExpanderSettingEntry);
            Loaded += (_, _) => Update();
        }

        public IconElement Icon
        {
            get => (IconElement) GetValue(IconProperty);
            set => SetValue(IconProperty, value);
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

        public double HeaderHeight
        {
            get => (double) GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        public object ActionContent
        {
            get => GetValue(ActionContentProperty);
            set => SetValue(ActionContentProperty, value);
        }

        public Thickness ContentMargin
        {
            get => (Thickness)GetValue(ContentMarginProperty);
            set => SetValue(ContentMarginProperty, value);
        }

        private void Update()
        {
            DescriptionChanged(this, Description);
            IconChanged(this, Icon);
        }

        protected override void OnApplyTemplate()
        {
            _entryHeader = GetTemplateChild(PartEntryHeader) as SettingEntryHeader;
            _entryContentPresenter = GetTemplateChild(PartEntryContentPresenter) as ContentPresenter;
            base.OnApplyTemplate();
        }

        private static void IconChanged(DependencyObject dependencyObject, object? argsNewValue)
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

        private static void DescriptionChanged(DependencyObject dependencyObject, object? argsNewValue)
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
}