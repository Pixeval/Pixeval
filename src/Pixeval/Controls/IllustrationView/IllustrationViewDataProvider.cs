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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;
using Pixeval.Utilities;

namespace Pixeval.Controls.IllustrationView;

/// <summary>
/// 复用时调用<see cref="CloneRef"/>，<see cref="FetchEngineRef"/>和<see cref="IllustrationSourceRef"/>会在所有复用对象都Dispose时Dispose。
/// 初始化时调用<see cref="ResetAndFillAsync"/>
/// </summary>
public class IllustrationViewDataProvider : DataProvider<Illustration, IllustrationViewModel>, IDisposable
{
    private SharedRef<IFetchEngine<Illustration?>?>? _fetchEngineRef;

    public SharedRef<IFetchEngine<Illustration?>?>? FetchEngineRef
    {
        get => _fetchEngineRef;
        private set
        {
            if (Equals(value, _fetchEngineRef))
                return;
            FetchEngine?.EngineHandle.Cancel();

            _fetchEngineRef = value;
        }
    }

    public override IFetchEngine<Illustration?>? FetchEngine
    {
        get => _fetchEngineRef?.Value;
        protected set => throw new NotImplementedException("No setter.");
    }

    public override AdvancedObservableCollection<IllustrationViewModel> View { get; } = [];

    private SharedRef<IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationViewModel>, IllustrationViewModel>> _illustrationSourceRef = null!;

    protected SharedRef<IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationViewModel>, IllustrationViewModel>> IllustrationSourceRef
    {
        get => _illustrationSourceRef;
        set
        {
            if (Equals(_illustrationSourceRef, value))
                return;

            OnPropertyChanging();
            if (_illustrationSourceRef is { } old)
            {
                old.Value.CollectionChanged -= OnIllustrationsSourceOnCollectionChanged;
                _ = old.TryDispose(this);
            }
            _illustrationSourceRef = value;
            value.Value.CollectionChanged += OnIllustrationsSourceOnCollectionChanged;
            View.Source = value.Value;
            OnPropertyChanged();
        }
    }

    public override IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationViewModel>, IllustrationViewModel> Source
    {
        get => _illustrationSourceRef.Value;
        protected set => throw new NotImplementedException("No setter.");
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

    public ObservableCollection<IllustrationViewModel> SelectedIllustrations { get; set; } = [];

    public override void DisposeCurrent()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (IllustrationSourceRef is not null)
        {
            Source.CollectionChanged -= OnIllustrationsSourceOnCollectionChanged;
            if (IllustrationSourceRef.TryDispose(this))
                foreach (var illustrationViewModel in Source)
                    illustrationViewModel.Dispose();
        }

        SelectedIllustrations.Clear();
    }

    public override async Task<int> ResetAndFillAsync(IFetchEngine<Illustration?>? fetchEngine, int limit = -1)
    {
        FetchEngineRef = new(fetchEngine, this);
        DisposeCurrent();

        IllustrationSourceRef = new(new(new IllustrationFetchEngineIncrementalSource(FetchEngine!, limit)), this);
        // TODO: 根据屏幕大小决定加载多少
        var result = await LoadMoreAsync(50);
        return (int)result;
    }

    public async Task<uint> LoadMoreAsync(uint count = 20)
    {
        return (await View.LoadMoreItemsAsync(count)).Count;
    }

    protected virtual void OnIllustrationsSourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e)
        {
            case { Action: NotifyCollectionChangedAction.Add }:
                e.NewItems?.OfType<IllustrationViewModel>().ForEach(i => i.IsSelectedChanged += OnIsSelectedChanged);
                break;
            case { Action: NotifyCollectionChangedAction.Remove }:
                e.NewItems?.OfType<IllustrationViewModel>().ForEach(i => i.IsSelectedChanged -= OnIsSelectedChanged);
                break;
        }

        return;

        void OnIsSelectedChanged(object? s, IllustrationViewModel model)
        {
            // Do not add to collection is the model does not conform to the filter
            if (!View.Filter?.Invoke(model) ?? false)
                return;
            if (model.IsSelected)
                SelectedIllustrations.Add(model);
            else
                _ = SelectedIllustrations.Remove(model);
        }
    }

    public void Dispose()
    {
        DisposeCurrent();
        FetchEngineRef = null;
    }

    ~IllustrationViewDataProvider() => Dispose();
}
