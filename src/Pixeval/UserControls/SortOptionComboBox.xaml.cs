using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Options;
using Pixeval.Util;

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

        private SelectionChangedEventHandler? _selectionChangedWhenLoadedInternal;

        public event SelectionChangedEventHandler SelectionChangedWhenLoaded
        {
            add => _selectionChangedWhenLoadedInternal += value;
            remove => _selectionChangedWhenLoadedInternal -= value;
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

        public SortDescription? GetSortDescription()
        {
            return MakoHelper.GetSortDescriptionForIllustration(SelectedOption);
        }

        private void SortOptionComboBox_OnSelectionChangedWhenPrepared(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = ComboBox.SelectedItem;
            _selectionChangedWhenLoadedInternal?.Invoke(sender, e);
        }
    }
}
