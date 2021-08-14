using System.Collections.Generic;
using CommunityToolkit.Common.Collections;
using Mako.Engine;

namespace Pixeval.Util
{
    public interface IIncrementalLoadingMappedItemSource<in TModel, TViewModel> : IIncrementalSource<TViewModel>
    {
        TViewModel? GetViewModel(TModel model);
    }
}