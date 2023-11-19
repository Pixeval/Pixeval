#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DataProvider.cs
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

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;

namespace Pixeval.Controls.Illustrate;

public abstract class DataProvider<T, TViewModel> : ObservableObject where T : IIllustrate where TViewModel : IllustrateViewModel<T>
{
    /// <summary>
    /// Avoid using <see cref="AdvancedCollectionView.Filter"/>, if you want to set the filter, use <see cref="Filter"/>
    /// </summary>
    public abstract AdvancedCollectionView View { get; }

    public abstract IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel> Source { get; protected set; }

    public abstract IFetchEngine<T?>? FetchEngine { get; protected set; }

    public abstract void DisposeCurrent();

    public abstract Task<int> ResetAndFillAsync(IFetchEngine<T?>? fetchEngine, int limit = -1);
}
