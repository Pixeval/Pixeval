using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Options;

namespace Pixeval.UserControls
{
    public sealed partial class SortOptionComboBox
    {
        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), 
            typeof(object), 
            typeof(SortOptionComboBox),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public SortOptionComboBox()
        {
            InitializeComponent();
        }

        private event SelectionChangedEventHandler? SelectionChangedInternal;

        public event SelectionChangedEventHandler SelectionChanged
        {
            add => SelectionChangedInternal += value;
            remove => SelectionChangedInternal -= value;
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set
            {
                SetValue(SelectedItemProperty, value);
                ComboBox.SelectedItem = value;
            }
        }

        public IllustrationSortOption SelectedOption => ((IllustrationSortOptionWrapper) SelectedItem).Value;

        private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChangedInternal?.Invoke(sender, e);
        }
    }
}
