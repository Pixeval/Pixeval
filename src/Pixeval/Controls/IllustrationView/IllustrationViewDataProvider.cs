#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationViewDataProvider.cs
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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// 复用时调用<see cref="CloneRef"/>，<see cref="FetchEngineRef"/>和<see cref="IllustrationSourceRef"/>会在所有复用对象都Dispose时Dispose。
/// 初始化时调用<see cref="ResetEngine"/>
/// </summary>
public class IllustrationViewDataProvider : ObservableObject, IDataProvider<Illustration, IllustrationItemViewModel>, IDisposable
{
    private SharedRef<IFetchEngine<Illustration?>?>? _fetchEngineRef;

    private SharedRef<IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationItemViewModel>, IllustrationItemViewModel>> _illustrationSourceRef = null!;

    public SharedRef<IFetchEngine<Illustration?>?>? FetchEngineRef
    {
        get => _fetchEngineRef;
        private set
        {
            if (Equals(value, _fetchEngineRef))
                return;
            if (_fetchEngineRef?.TryDispose(this) is true)
                FetchEngine?.EngineHandle.Cancel();
            _fetchEngineRef = value;
        }
    }

    protected SharedRef<IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationItemViewModel>, IllustrationItemViewModel>> IllustrationSourceRef
    {
        get => _illustrationSourceRef;
        set
        {
            if (Equals(_illustrationSourceRef, value))
                return;

            DisposeIllustrationSourceRef();
            _illustrationSourceRef = value;
            View.Source = value.Value;
            OnPropertyChanged();
        }
    }

    public IFetchEngine<Illustration?>? FetchEngine => _fetchEngineRef?.Value;

    public AdvancedObservableCollection<IllustrationItemViewModel> View { get; } = [];

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationItemViewModel>, IllustrationItemViewModel> Source => _illustrationSourceRef.Value;

    public void Dispose()
    {
        DisposeIllustrationSourceRef();
        // 赋值为null会自动调用setter中的Dispose逻辑
        FetchEngineRef = null;
    }

    public void ResetEngine(IFetchEngine<Illustration?>? fetchEngine, int limit = -1)
    {
        FetchEngineRef = new SharedRef<IFetchEngine<Illustration?>?>(fetchEngine, this);
        IllustrationSourceRef = new SharedRef<IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationItemViewModel>, IllustrationItemViewModel>>(new IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationItemViewModel>, IllustrationItemViewModel>(new IllustrationFetchEngineIncrementalSource(FetchEngine!, limit)), this);
    }

    public IllustrationViewDataProvider CloneRef()
    {
        var dataProvider = new IllustrationViewDataProvider();
        dataProvider.FetchEngineRef = FetchEngineRef?.MakeShared(dataProvider);
        dataProvider.IllustrationSourceRef = IllustrationSourceRef.MakeShared(dataProvider);
        dataProvider.View.Filter = View.Filter;
        foreach (var viewSortDescription in View.SortDescriptions)
            dataProvider.View.SortDescriptions.Add(viewSortDescription);
        return dataProvider;
    }

    private void DisposeIllustrationSourceRef()
    {
        if (_illustrationSourceRef?.TryDispose(this) is true)
            foreach (var illustrationViewModel in Source)
                illustrationViewModel.Dispose();
    }

    ~IllustrationViewDataProvider() => Dispose();
}
