#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/GridIllustratorViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.UserControls.IllustratorView;

public sealed class GridIllustratorViewViewModel : IllustratorViewViewModel
{
    private readonly IDictionary<string, (Task<SoftwareBitmapSource[]>, CancellationTokenSource)> _illustratorDisplayImageSourceCache;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public GridIllustratorViewViewModel()
    {
        DataProvider = new GridIllustratorViewDataProvider();
        _illustratorDisplayImageSourceCache = new Dictionary<string, (Task<SoftwareBitmapSource[]>, CancellationTokenSource)>();
    }


    public override IIllustratorViewDataProvider DataProvider { get; }

    public override async Task<ImageSource[]> GetIllustratorDisplayImagesAsync(string userId)
    {
        async Task<SoftwareBitmapSource[]> GetImagesAsync(CancellationToken token)
        {
            var list = new List<Task<Result<ImageSource>>>();
            var counter = 0;
            await foreach (var i in App.AppViewModel.MakoClient.Posts(userId).Where(i => i.GetThumbnailUrl(ThumbnailUrlOption.SquareMedium) is not null).WithCancellation(token))
            {
                if (counter >= 3) break;
                counter++;

                list.Add(App.AppViewModel.MakoClient.DownloadSoftwareBitmapSourceResultAsync(i.GetThumbnailUrl(ThumbnailUrlOption.SquareMedium)!));
            }

            var results = (await Task.WhenAll(list)).Select(result => result switch
            {
                Result<ImageSource>.Success(SoftwareBitmapSource source) => source,
                Result<ImageSource>.Failure => null,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            }).WhereNotNull().Take(3).ToArray();

            if (token.IsCancellationRequested)
            {
                results.ForEach(r => r.Dispose());
                token.ThrowIfCancellationRequested();
            }

            return results;
        }

        await _semaphoreSlim.WaitAsync();
        var cancellationTokenSource = new CancellationTokenSource();
        var (images, _) = _illustratorDisplayImageSourceCache.GetOrCreate(userId, () => (GetImagesAsync(cancellationTokenSource.Token), cancellationTokenSource));
        _semaphoreSlim.Release();
        return await images;
    }

    public override void Dispose()
    {
        foreach (var (_, (task, cancellationTokenSource)) in _illustratorDisplayImageSourceCache)
        {
            if (task.IsCompletedSuccessfully)
            {
                task.Result.ForEach(s => s.Dispose());
            }
            else
            {
                cancellationTokenSource.Cancel();
            }
        }
        DataProvider.FetchEngine?.Cancel();
        DataProvider.DisposeCurrent();
    }
}