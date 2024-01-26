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

using System;
using System.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.Misc;

namespace Pixeval.Controls.Illustrate;

public interface IDataProvider<T, TViewModel> : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable where T : class where TViewModel : class
{
    AdvancedObservableCollection<TViewModel> View { get; }

    IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel> Source { get; }

    IFetchEngine<T?>? FetchEngine { get; }

    void ResetEngine(IFetchEngine<T?>? fetchEngine, int limit = -1);
}
