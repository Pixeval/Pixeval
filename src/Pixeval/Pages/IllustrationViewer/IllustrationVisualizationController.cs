using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.UserControls;
using Pixeval.Util;

namespace Pixeval.Pages.IllustrationViewer;

public class IllustrationVisualizationController : IDisposable
{
    private readonly IIllustrationVisualizer _visualizer;

    public IFetchEngine<Illustration?>? FetchEngine { get; set; }

    public IllustrationVisualizationController(IIllustrationVisualizer visualizer)
    {
        _visualizer = visualizer;
    }

    public async Task FillAsync(int? itemsLimit = null)
    {
        var added = new HashSet<long>();
        await foreach (var illustration in FetchEngine!)
        {
            if (illustration is not null && !added.Contains(illustration.Id) /* Check for the repetition */)
            {
                if (added.Count >= itemsLimit)
                {
                    FetchEngine.Cancel();
                    break;
                }

                added.Add(illustration.Id); // add to the already-added-illustration list
                _visualizer.AddIllustrationViewModel(new IllustrationViewModel(illustration));
            }
        }
    }

    public async Task FillAsync(IFetchEngine<Illustration?>? newEngine, int? itemsLimit = null)
    {
        FetchEngine = newEngine;
        await FillAsync(itemsLimit);
    }

    public async Task ResetAndFillAsync(IFetchEngine<Illustration?>? newEngine, int? itemLimit = null)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = newEngine;
        _visualizer.DisposeCurrent();
        await FillAsync(itemLimit);
    }

    public void Dispose()
    {
        _visualizer.DisposeCurrent();
        FetchEngine = null;
    }
}