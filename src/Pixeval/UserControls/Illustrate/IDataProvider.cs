using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.UserControls.Illustrate;

public interface IDataProvider<T, TViewModel> where T : IIllustrate where TViewModel : IllustrateViewModel<T>
{
    /// <summary>
    /// Avoid using <see cref="AdvancedCollectionView.Filter"/>, if you want to set the filter, use <see cref="Filter"/>
    /// </summary>
    AdvancedCollectionView View { get; }

    ObservableCollection<TViewModel> Source { get; }

    IFetchEngine<T?>? FetchEngine { get; }

    Predicate<object>? Filter { get; set; }

    event EventHandler FilterChanged;

    void DisposeCurrent();

    Task<int> ResetAndFillAsync(IFetchEngine<T?>? fetchEngine, int itemLimit = -1);
}
