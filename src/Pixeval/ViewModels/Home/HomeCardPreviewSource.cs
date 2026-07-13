// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;

namespace Pixeval.ViewModels.Home;

public sealed class HomeCardPreviewSource(
    ISimpleViewViewModel viewModel,
    object? openingContext = null) : IRefCloneable<HomeCardPreviewSource>, IDisposable
{
    private ISimpleViewViewModel? _viewModel = viewModel;

    public ISimpleViewViewModel ViewModel =>
        _viewModel ?? throw new ObjectDisposedException(nameof(HomeCardPreviewSource));

    public object? OpeningContext { get; } = openingContext;

    public HomeCardPreviewSource CloneRef() =>
        ViewModel is IRefCloneable<ISimpleViewViewModel> cloneable
            ? new(cloneable.CloneRef(), OpeningContext)
            : throw new InvalidOperationException($"The home card view model {ViewModel.GetType().Name} cannot be cloned.");

    public TViewModel GetViewModel<TViewModel>()
        where TViewModel : class, ISimpleViewViewModel =>
        ViewModel as TViewModel
        ?? throw new InvalidOperationException($"The home card does not provide a {typeof(TViewModel).Name}.");

    public TViewModel TakeViewModel<TViewModel>()
        where TViewModel : class, ISimpleViewViewModel
    {
        var viewModel = GetViewModel<TViewModel>();
        _viewModel = null;
        return viewModel;
    }

    public TContext GetOpeningContext<TContext>()
        where TContext : class =>
        OpeningContext as TContext
        ?? throw new InvalidOperationException($"The home card does not provide a {typeof(TContext).Name} opening context.");

    public void Dispose()
    {
        var viewModel = _viewModel;
        _viewModel = null;
        if (viewModel is IDisposable disposable)
            disposable.Dispose();
    }
}
