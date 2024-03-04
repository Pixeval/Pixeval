#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SpotlightArticleViewDataProvider.cs
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls.SpotlightArticleView;
public class SpotlightArticleViewDataProvider : ObservableObject, IDataProvider<SpotlightArticle, SpotlightArticleViewModel>
{
    public AdvancedObservableCollection<SpotlightArticleViewModel> View { get; } = [];

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<SpotlightArticle, SpotlightArticleViewModel>, SpotlightArticleViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<FetchEngineIncrementalSource<SpotlightArticle, SpotlightArticleViewModel>, SpotlightArticleViewModel>)!;
        protected set => View.Source = value;
    }

    public IFetchEngine<SpotlightArticle?>? FetchEngine { get; protected set; }

    public void Dispose()
    {
        if (Source is { } source)
            foreach (var illustratorViewModel in source)
            {
                illustratorViewModel.Dispose();
            }

        View.Clear();
    }

    public void ResetEngine(IFetchEngine<SpotlightArticle?>? fetchEngine, int limit = -1)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = fetchEngine;
        Dispose();

        Source = new IncrementalLoadingCollection<FetchEngineIncrementalSource<SpotlightArticle, SpotlightArticleViewModel>, SpotlightArticleViewModel>(new SpotlightArticleFetchEngineIncrementalSource(FetchEngine!, limit));
    }
}
