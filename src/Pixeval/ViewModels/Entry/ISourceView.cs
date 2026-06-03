// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public interface ISourceView<TViewModel>
    : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
    where TViewModel : class
{
    IAdvancedObservableView<TViewModel> View { get; }

    ObservableCollection<TViewModel> Source { get; }
}
