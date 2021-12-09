#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ISortedIllustrationContainerPageHelper.cs
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

using Pixeval.UserControls;

namespace Pixeval.Misc;

public interface ISortedIllustrationContainerPageHelper
{
    IllustrationContainer ViewModelProvider { get; }

    SortOptionComboBox SortOptionProvider { get; }

    public void OnSortOptionChanged()
    {
        switch (SortOptionProvider.GetSortDescription())
        {
            case { } desc:
                ViewModelProvider.ViewModel.SetSortDescription(desc);
                ViewModelProvider.ScrollToTop();
                break;
            default:
                // reset the view so that it can resort its item to the initial order
                ViewModelProvider.ViewModel.ClearSortDescription();
                ViewModelProvider.ViewModel.IllustrationsView.Refresh();
                ViewModelProvider.ScrollToTop();
                break;
        }
    }
}