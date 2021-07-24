using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Mako.Global.Enum;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    public sealed partial class RecommendsPage
    {
        public RecommendsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            PrivacyPolicyComboBox.SelectedItem = PrivacyPolicyComboBoxIllustItem;
            SortOptionComboBox.SelectedItem = SortOptionComboBoxPublishDateDescendingItem;
        }

        private async void RecommendsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ChangeSource();
        }

        private async void PrivacyPolicyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ChangeSource();
        }

        private async void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ChangeSource();
        }

        private async Task ChangeSource()
        {
            if (TryGetRecommendContentType(PrivacyPolicyComboBox, out var type) && TryGetIllustrationSortOption(SortOptionComboBox, out var option))
            {
                await IllustrationGrid.ViewModel.ResetAndFill(App.MakoClient.Recommends(type), GetInsertAction(option));
            }
        }

        #region Helper Functions

        private static bool TryGetRecommendContentType(ComboBox sender, out RecommendContentType type)
        {
            if (sender is {SelectedItem: ComboBoxItem {Tag: RecommendContentType t}})
            {
                type = t;
                return true;
            }

            type = RecommendContentType.Illust;
            return false;
        }

        private static bool TryGetIllustrationSortOption(object sender, out IllustrationSortOption type)
        {
            if (sender is ComboBox {SelectedItem: ComboBoxItem {Tag: IllustrationSortOption t}})
            {
                type = t;
                return true;
            }

            type = IllustrationSortOption.PublishDateDescending;
            return false;
        }

        private static Action<ObservableCollection<IllustrationViewModel>, IllustrationViewModel?> GetInsertAction(IllustrationSortOption sortOption)
        {
            return sortOption switch
            {
                IllustrationSortOption.PublishDateAscending  => (models, model) => models!.AddSorted(model, (m1, m2) => IllustrationViewModel.Compare(m1, m2, m => m.PublishDate)),
                IllustrationSortOption.PublishDateDescending => (models, model) => models!.AddSorted(model, (m1, m2) => -IllustrationViewModel.Compare(m1, m2, m => m.PublishDate)),
                IllustrationSortOption.PopularityDescending  => (models, model) => models!.AddSorted(model, (m1, m2) => -IllustrationViewModel.Compare(m1, m2, m => m.Bookmark)),
                _                                            => throw new ArgumentOutOfRangeException(nameof(sortOption), sortOption, null)
            };
        }

        #endregion
    }
}
