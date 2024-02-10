#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IIllustratorContentViewerSubPage.cs
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
using Pixeval.Misc;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public abstract class IllustratorContentViewerSubPage : EnhancedPage, ISortedIllustrationContainerPageHelper
{
    public abstract IllustrationContainer ViewModelProvider { get; }

    public abstract SortOptionComboBox SortOptionProvider { get; }

    public void PerformSearch(string keyword)
    {
        if (ViewModelProvider.ShowCommandBar)
        {
            return;
        }

        ViewModelProvider.ViewModel.DataProvider.View.Filter = keyword.IsNullOrBlank()
            ? null
            : o => o.Id.ToString().Contains(keyword)
                   || o.Illustrate.Tags.Any(x =>
                       x.Name.Contains(keyword) || (x.TranslatedName?.Contains(keyword) ?? false))
                   || o.Illustrate.Title.Contains(keyword);
    }

    protected void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((ISortedIllustrationContainerPageHelper)this).OnSortOptionChanged();
    }

    public sealed override void OnPageActivated(NavigationEventArgs e)
    {
        if (ActivationCount > 1)
            return;

        if (e.Parameter is long id)
        {
            OnPageActivated(id);
        }

        if (!App.AppViewModel.AppSetting.ShowExternalCommandBarInIllustratorContentViewer)
        {
            ViewModelProvider.ShowCommandBar = false;
        }
    }

    public abstract void OnPageActivated(long id);
}
