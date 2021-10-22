#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/AdvancedCollectionView.Events.cs
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

using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Data;

namespace Pixeval.CommunityToolkit.AdvancedCollectionView
{
    /// <summary>
    ///     A collection view implementation that supports filtering, grouping, sorting and incremental loading
    /// </summary>
    public partial class AdvancedCollectionView
    {
        /// <summary>
        ///     Currently selected item changing event
        /// </summary>
        /// <param name="e">event args</param>
        private void OnCurrentChanging(CurrentChangingEventArgs e)
        {
            if (_deferCounter > 0)
            {
                return;
            }

            CurrentChanging?.Invoke(this, e);
        }

        /// <summary>
        ///     Currently selected item changed event
        /// </summary>
        /// <param name="e">event args</param>
        private void OnCurrentChanged(object? e)
        {
            if (_deferCounter > 0)
            {
                return;
            }

            CurrentChanged?.Invoke(this, e);

            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(CurrentItem));
        }

        /// <summary>
        ///     Vector changed event
        /// </summary>
        /// <param name="e">event args</param>
        private void OnVectorChanged(IVectorChangedEventArgs e)
        {
            if (_deferCounter > 0)
            {
                return;
            }

            VectorChanged?.Invoke(this, e);

            OnPropertyChanged(nameof(Count));
        }
    }
}