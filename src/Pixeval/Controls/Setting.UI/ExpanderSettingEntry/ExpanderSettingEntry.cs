using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using Pixeval.Controls.Setting.UI.UserControls;

namespace Pixeval.Controls.Setting.UI.ExpanderSettingEntry
{
    [ContentProperty(Name = nameof(Content))]
    [TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
    public class ExpanderSettingEntry : SettingEntryBase
    {
        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
            nameof(HeaderHeight),
            typeof(double),
            typeof(ExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public double HeaderHeight
        {
            get => (double) GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content),
            typeof(object),
            typeof(ExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(
            nameof(ContentMargin),
            typeof(Thickness),
            typeof(ExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public Thickness ContentMargin
        {
            get => (Thickness) GetValue(ContentMarginProperty);
            set => SetValue(ContentMarginProperty, value);
        }

        public ExpanderSettingEntry()
        {
            DefaultStyleKey = typeof(ExpanderSettingEntry);
        }
    }
}