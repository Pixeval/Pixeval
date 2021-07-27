using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Mako.Global.Enum;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util;
using Pixeval.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BookMarksPage : Page
    {
        public BookMarksPage()
        {
            InitializeComponent();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            PrivacyPolicyComboBox.SelectedItem = PrivacyPolicyComboBoxPublicItem;
            SortOptionComboBox.SelectedItem = SortOptionComboBoxPublishDateDescendingComboBoxItem;
        }

        private async void BookMarksPage_OnLoaded(object sender, RoutedEventArgs e)
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
            if (TryGetPrivacyPolicy(PrivacyPolicyComboBox, out var type)
                && TryGetIllustrationSortOption(SortOptionComboBox, out var option))
            {
                await IllustrationGrid.ViewModel.ResetAndFill(App.MakoClient.Bookmarks(App.Global.Uid!, type), GetInsertAction(option));
            }
        }

        #region Helper Functions

        private static bool TryGetPrivacyPolicy(ComboBox sender, out PrivacyPolicy type)
        {
            if (sender is { SelectedItem: ComboBoxItem { Tag: PrivacyPolicy t } })
            {
                type = t;
                return true;
            }

            type = PrivacyPolicy.Public;
            return false;
        }

        private static bool TryGetIllustrationSortOption(object sender, out IllustrationSortOption type)
        {
            if (sender is ComboBox { SelectedItem: ComboBoxItem { Tag: IllustrationSortOption t } })
            {
                type = t;
                return true;
            }

            type = IllustrationSortOption.PublishDateDescending;
            return false;
        }

        private static Action<ObservableCollection<IllustrationViewModel>, IllustrationViewModel?> GetInsertAction(
            IllustrationSortOption sortOption)
        {
            return sortOption switch
            {
                IllustrationSortOption.PublishDateAscending => (models, model) =>
                    models!.AddSorted(model, (m1, m2) => IllustrationViewModel.Compare(m1, m2, m => m.PublishDate)),
                IllustrationSortOption.PublishDateDescending => (models, model) =>
                    models!.AddSorted(model, (m1, m2) => -IllustrationViewModel.Compare(m1, m2, m => m.PublishDate)),
                IllustrationSortOption.PopularityDescending => (models, model) =>
                    models!.AddSorted(model, (m1, m2) => -IllustrationViewModel.Compare(m1, m2, m => m.Bookmark)),
                _ => throw new ArgumentOutOfRangeException(nameof(sortOption), sortOption, null)
            };
        }

        #endregion
    }
}