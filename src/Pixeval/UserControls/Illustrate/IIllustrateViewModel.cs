using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Model;

namespace Pixeval.UserControls.Illustrate;

public abstract class IllustrateViewModel<T>(T illustrate) : ObservableObject, IDisposable
    where T : IIllustrate
{
    public T Illustrate { get; } = illustrate;

    public abstract void Dispose();
}
