#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/Upscaler.cs
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
using System.Diagnostics;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using Windows.Storage;
using Pixeval.AppManagement;
using Pixeval.Utilities.Threading;

namespace Pixeval.Upscaling;

public partial class Upscaler(UpscaleTask task) : IDisposable, IAsyncDisposable
{
    public const string ProcessCompletedMark = "Completed";

    private static string ExecutablePath => AppKnownFolders.Installation.Resolve(@"Assets\Binary\RealESRGAN\realesrgan-ncnn-vulkan.exe");

    private Stream? _upscaleStream;

    private readonly ReenterableAwaiter<bool> _runningSignal = new(false, true);

    private bool _isDisposed;

    public async Task<Stream> UpscaleAsync(ChannelWriter<string> messageChannel)
    {
        if (_isDisposed)
        {
            throw new InvalidOperationException("This upscaler is already disposed");
        }

        await _runningSignal.ResetAsync();

        var id = Guid.NewGuid().ToString();

        var tempFilePath = await AppKnownFolders.Temporary.CreateFileAsync(id, CreationCollisionOption.GenerateUniqueName);
        _ = task.ImageStream.Seek(0, SeekOrigin.Begin);

        // scoped-using is obligatory here, otherwise the file will be locked and the process will not be able to access it
        await using (var tempStream = await tempFilePath.OpenStreamForWriteAsync())
        {
            await task.ImageStream.CopyToAsync(tempStream);
        }

        _ = task.ImageStream.Seek(0, SeekOrigin.Begin);

        var modelParam = task.Model switch
        {
            RealESRGANModel.RealESRGANX4Plus => "realesrgan-x4plus",
            RealESRGANModel.RealESRNETX4Plus => "realesrnet-x4plus",
            RealESRGANModel.RealESRGANX4PlusAnime => "realesrgan-x4plus-anime",
            _ => throw new ArgumentOutOfRangeException()
        };

        var outputType = task.OutputType switch
        {
            UpscalerOutputType.Png => "png",
            UpscalerOutputType.Jpeg => "jpg",
            UpscalerOutputType.WebP => "webp",
            _ => throw new ArgumentOutOfRangeException()
        };

        var outputFilePath = AppKnownFolders.Temporary.Resolve($"{id}_out.{outputType}");

        var process = new Process();
        process.StartInfo.FileName = ExecutablePath;
        process.StartInfo.Arguments = $"-i {tempFilePath.Path} -o {outputFilePath} -n {modelParam} -s {task.ScaleRatio}";
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;

        _ = process.Start();

        _ = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream)
            {
                await messageChannel.WriteAsync(await process.StandardError.ReadLineAsync() ?? string.Empty);
            }
        });

        await process.WaitForExitAsync();
        await messageChannel.WriteAsync(ProcessCompletedMark);

        _upscaleStream = await (await AppKnownFolders.Temporary.GetFileAsync($"{id}_out.{outputType}")).OpenStreamForReadAsync();
        await _runningSignal.SetResultAsync(true);
        return _upscaleStream;
    }

    public async void Dispose()
    {
        _isDisposed = true;
        _ = await _runningSignal;

        GC.SuppressFinalize(this);
        if (_upscaleStream != null)
            await _upscaleStream.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        _ = await _runningSignal;

        GC.SuppressFinalize(this);
        if (_upscaleStream != null) 
            await _upscaleStream.DisposeAsync();
    }
}
