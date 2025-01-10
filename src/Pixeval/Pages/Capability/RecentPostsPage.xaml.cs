// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Pages.Capability;

public sealed partial class RecentPostsPage : IScrollViewHost
{
    public RecentPostsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e) => ChangeSource();

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        var privacyPolicy = PrivacyPolicyComboBox.GetSelectedItem<PrivacyPolicy>();
        WorkContainer.WorkView.ResetEngine(SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>() is SimpleWorkType.IllustAndManga
            ? App.AppViewModel.MakoClient.RecentIllustrationPosts(privacyPolicy)
            : App.AppViewModel.MakoClient.RecentNovelPosts(privacyPolicy));
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;
}
