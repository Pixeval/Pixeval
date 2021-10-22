#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/AdvancedCollectionView.Defer.cs
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

using System;

namespace Pixeval.CommunityToolkit.AdvancedCollectionView
{
    /// <summary>
    ///     A collection view implementation that supports filtering, grouping, sorting and incremental loading
    /// </summary>
    public partial class AdvancedCollectionView
    {
        /// <summary>
        ///     Stops refreshing until it is disposed
        /// </summary>
        /// <returns>An disposable object</returns>
        public IDisposable DeferRefresh()
        {
            return new NotificationDeferrer(this);
        }

        /// <summary>
        ///     Notification deferrer helper class
        /// </summary>
        public class NotificationDeferrer : IDisposable
        {
            private readonly AdvancedCollectionView _acvs;
            private readonly object? _currentItem;

            /// <summary>
            ///     Initializes a new instance of the <see cref="NotificationDeferrer" /> class.
            /// </summary>
            /// <param name="acvs">Source ACVS</param>
            public NotificationDeferrer(AdvancedCollectionView acvs)
            {
                _acvs = acvs;
                _currentItem = _acvs.CurrentItem;
                _acvs._deferCounter++;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                _acvs.MoveCurrentTo(_currentItem);
                _acvs._deferCounter--;
                _acvs.Refresh();
            }
        }
    }
}