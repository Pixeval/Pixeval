#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ISortedIllustrationContainerPageHelper.cs
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

using Pixeval.Controls;

namespace Pixeval.Misc;

/// <summary>
/// A base class for all the pages whose illustration contents are sortable, call <see cref="OnSortOptionChanged"/> to manually update
/// the sort condition and refresh the view
/// </summary>
public interface ISortedIllustrationContainerPageHelper
{
    /// <summary>
    /// The <see cref="IllustrationContainer"/> that contains the illustration contents of this page
    /// </summary>
    IllustrationContainer ViewModelProvider { get; }

    /// <summary>
    /// The <see cref="SortOptionComboBox"/> that provides several order options.
    /// </summary>
    SortOptionComboBox SortOptionProvider { get; }

    public void OnSortOptionChanged()
    {
        if (ViewModelProvider.ViewModel is { } vm)
        {
            switch (SortOptionProvider.GetSortDescription())
            {
                case { } desc:
                    vm.SetSortDescription(desc);
                    ViewModelProvider.ScrollToTop();
                    break;
                default:
                    // reset the view so that it can resort its item to the initial order
                    vm.ClearSortDescription();
                    ViewModelProvider.ScrollToTop();
                    break;
            }
        }
    }
}
