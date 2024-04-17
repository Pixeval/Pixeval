#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RecommendationPage.xaml.cs
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

using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendationPage
{
    public RecommendationPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e) => ChangeSource();

    private void WorkTypeComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => ChangeSource();

    private async void ChangeSource()
    {
        var recommendationWorks = App.AppViewModel.MakoClient.RecommendationWorks(WorkType.Manga, App.AppViewModel.AppSettings.TargetFilter);

        await foreach (var recommendationWork in recommendationWorks)
        {
            if (recommendationWork is Illustration{ })
                WorkContainer.WorkView.Add(recommendationWork)
        }

        WorkContainer.WorkView.ResetEngine(App.AppViewModel.MakoClient.RecommendationWorks(WorkTypeComboBox.GetSelectedItem<WorkType>(), App.AppViewModel.AppSettings.TargetFilter));
    }
}
