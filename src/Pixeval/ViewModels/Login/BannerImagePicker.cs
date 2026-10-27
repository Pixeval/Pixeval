using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Models.Database.Managers;

namespace Pixeval.Views.Login;

public class BannerImagePicker(CancellationToken? token = null)
{
    private static async Task<T?> PickRandom<T>(
        IAsyncEnumerable<T> source,
        CancellationToken token = default)
    {
        T? selected = default;
        long count = 0;

        await foreach (var item in source.WithCancellation(token))
        {
            if (Random.Shared.NextInt64(++count) == 0)
                selected = item;
        }

        return selected;
    }
    
    private async Task<string?> LocateAsync()
    {
        var manager = App.AppViewModel.AppServiceProvider
            .GetRequiredService<DownloadHistoryPersistentManager>();

        var randomImage = await PickRandom(manager.StreamTaskGroupsAsync(token: token ?? CancellationToken.None),
            token ?? CancellationToken.None);
        if (randomImage is not null)
        {
            var location = randomImage.OpenLocalDestination;
            return Directory.Exists(location) 
                ? Directory.EnumerateFiles(location).FirstOrDefault(f => Path.GetExtension(f) is ".jpg" or ".png" or ".jpeg") 
                : location;
        }

        return null;
    }

    public async Task<Bitmap?> LoadRandomImageAsync()
    {
        return await LocateAsync().ConfigureAwait(false) is { } path
            ? new Bitmap(path)
            : null;
    }
}

