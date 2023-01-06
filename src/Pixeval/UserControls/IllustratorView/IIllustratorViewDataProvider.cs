#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IIllustratorViewDataProvider.cs
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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.UserControls.IllustratorView;

public interface IIllustratorViewDataProvider
{
    AdvancedCollectionView IllustratorsView { get; }

    ObservableCollection<IllustratorViewModel> IllustratorsSource { get; }

    IFetchEngine<User?>? FetchEngine { get; }

    Predicate<object>? Filter { get; }

    event EventHandler FilterChanged;

    void DisposeCurrent();

    Task<int> LoadMore();

    Task<int> FillAsync(int? itemsLimit = null);

    Task<int> ResetAndFillAsync(IFetchEngine<User?>? fetchEngine, int? itemLimit = null);
}