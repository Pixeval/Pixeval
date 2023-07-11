using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Model;

namespace Pixeval.UserControls.Illustrate;

public abstract class IllustrateViewModel<T> : ObservableObject, IDisposable where T : IIllustrate
{
    public T Illustrate { get; }
    
    protected IllustrateViewModel(T illustrate)
    {
        Illustrate = illustrate;
    }

    public abstract void Dispose();
}
