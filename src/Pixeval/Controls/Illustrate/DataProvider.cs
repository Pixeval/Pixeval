using System;
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

    public abstract Predicate<object>? Filter { get; set; }

    public abstract event EventHandler FilterChanged;

    public abstract void DisposeCurrent();

    public abstract Task<int> ResetAndFillAsync(IFetchEngine<T?>? fetchEngine, int limit = -1);
}
