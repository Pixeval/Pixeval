using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.Setting.UI.UserControls;

namespace Pixeval.Controls.Setting.UI.SwitchSettingEntry
{
    [TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
    [TemplatePart(Name = PartSwitch, Type = typeof(ToggleSwitch))]
    public class SwitchSettingEntry : SettingEntryBase
    {
        private const string PartSwitch = "Switch";
        private const string PartContentContainerGrid = "ContentContainerGrid";

        private ToggleSwitch? _switch;
        private Grid? _contentContainerGrid;

        private TypedEventHandler<SwitchSettingEntry, RoutedEventArgs>? _toggled;

        public event TypedEventHandler<SwitchSettingEntry, RoutedEventArgs> Toggled
        {
            add => _toggled += value;
            remove => _toggled -= value;
        }

        public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
            nameof(IsOn),
            typeof(bool),
            typeof(SwitchSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) => IsOnChanged(o, args.NewValue)));

        public bool IsOn
        {
            get => (bool) GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        private static void IsOnChanged(DependencyObject d, object newValue)
        {
            if (d is SwitchSettingEntry {_switch: { } sh})
            {
                sh.IsOn = (bool) newValue;
            }
        }

        private void SwitchOnToggled(object sender, RoutedEventArgs e)
        {
            IsOn = _switch!.IsOn;
            _toggled?.Invoke(this, e);
        }

        protected override void Update()
        {
            IsOnChanged(this, IsOn);
        }

        public SwitchSettingEntry()
        {
            DefaultStyleKey = typeof(SwitchSettingEntry);
        }

        protected override void OnApplyTemplate()
        {
            if (_switch is not null)
            {
                _switch.Toggled -= SwitchOnToggled;
            }

            if ((_switch = GetTemplateChild(PartSwitch) as ToggleSwitch) is not null)
            {
                _switch.Toggled += SwitchOnToggled;
            }

            _contentContainerGrid = GetTemplateChild(PartContentContainerGrid) as Grid;
            
            base.OnApplyTemplate();
        }
    }
}