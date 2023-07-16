using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;

namespace Pixeval.UserControls.Illustrate;

public interface IDataProvider<T, TViewModel> where T : IIllustrate where TViewModel : IllustrateViewModel<T>
{
    /// <summary>
    /// Avoid using <see cref="AdvancedCollectionView.Filter"/>, if you want to set the filter, use <see cref="Filter"/>
    /// </summary>
    AdvancedCollectionView View { get; }

    IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel> Source { get; }

    IFetchEngine<T?>? FetchEngine { get; }

    Predicate<object>? Filter { get; set; }

    event EventHandler FilterChanged;

    void DisposeCurrent();

    Task<int> ResetAndFillAsync(IFetchEngine<T?>? fetchEngine, int itemLimit = -1);
}
