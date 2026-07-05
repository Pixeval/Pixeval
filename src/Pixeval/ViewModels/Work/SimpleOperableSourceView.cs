// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mako.Model;
using Misaki;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public sealed class SimpleOperableSourceView<TViewModel>(IReadOnlyCollection<IArtworkInfo> source)
    : ViewModelBase, ISourceView<IWorkViewModel>
    where TViewModel : class, IWorkViewModel
{
    private bool _isDisposed;

    public AdvancedObservableAdaptor<IArtworkInfo, IWorkViewModel> View { get; } = new(source as ObservableCollection<IArtworkInfo> ?? [.. source], CreateWorkViewModel);

    IAdvancedObservableView<IWorkViewModel> ISourceView<IWorkViewModel>.View => View;

    public ObservableCollection<IWorkViewModel> Source => View.MappedSource;

    public ISourceView<TViewModel> CloneSourceView()
        => new SnapshotSourceView<TViewModel>(View.OfType<TViewModel>().Select(CloneItem));

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        View.Dispose();
    }

    private static IWorkViewModel CreateWorkViewModel(IArtworkInfo info) => info is Novel novel ? new NovelItemViewModel(novel) : new IllustrationItemViewModel(info);

    private static TViewModel CloneItem(TViewModel viewModel)
        => viewModel switch
        {
            IllustrationItemViewModel illustration => (TViewModel) (IWorkViewModel) IllustrationItemViewModel.CreateInstance(illustration.Entry),
            NovelItemViewModel novel => (TViewModel) (IWorkViewModel) NovelItemViewModel.CreateInstance(novel.Entry),
            _ => throw new NotSupportedException($"Unsupported simple work view model type: {typeof(TViewModel)}")
        };

    private sealed class SnapshotSourceView<TSnapshotViewModel>(IEnumerable<TSnapshotViewModel> source) : ViewModelBase, ISourceView<TSnapshotViewModel>
        where TSnapshotViewModel : class
    {
        private bool _isDisposed;

        public AdvancedObservableCollection<TSnapshotViewModel> View { get; } = new([.. source]);

        IAdvancedObservableView<TSnapshotViewModel> ISourceView<TSnapshotViewModel>.View => View;

        public ObservableCollection<TSnapshotViewModel> Source => View.Source;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            View.Dispose();
        }
    }
}
