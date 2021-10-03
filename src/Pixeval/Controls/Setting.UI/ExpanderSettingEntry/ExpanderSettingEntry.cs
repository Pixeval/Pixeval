using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Pixeval.Controls.Setting.UI.UserControls;

namespace Pixeval.Controls.Setting.UI.ExpanderSettingEntry
{
    [ContentProperty(Name = nameof(Content))]
    [TemplatePart(Name = PartEntryContentPresenter, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
    public class ExpanderSettingEntry : SettingEntryBase
    {
        private const string PartEntryContentPresenter = "EntryContentPresenter";

        private ContentPresenter? _entryContentPresenter;

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

        protected override void IconChanged(object? newValue)
        {
            if (_entryContentPresenter is { } presenter)
            {
                presenter.Margin = newValue is IconElement
                    ? new Thickness(50, 0, 50, 0)
                    : new Thickness(10, 0, 10, 0);
            }

            base.IconChanged(newValue);
        }

        protected override void OnApplyTemplate()
        {
            _entryContentPresenter = GetTemplateChild(PartEntryContentPresenter) as ContentPresenter;
            base.OnApplyTemplate();
        }

        public ExpanderSettingEntry()
        {
            DefaultStyleKey = typeof(ExpanderSettingEntry);
        }
    }
}