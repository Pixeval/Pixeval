// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pixeval.Utilities;

namespace Pixeval.ViewModels.Search;

public sealed partial class SauceNaoSearchPageViewModel : ViewModelBase, IDisposable
{
    public ReadOnlyMemory<byte> File { get; private set; } = ReadOnlyMemory<byte>.Empty;

    [ObservableProperty]
    public partial Bitmap? Image { get; private set; }

    [ObservableProperty]
    public partial bool FileSelected { get; private set; }

    public static string ApiKey => App.AppViewModel.AppSettings.SearchSettings.ReverseSearchApiKey;

    public static bool IsApiKeyExisted => !string.IsNullOrWhiteSpace(ApiKey);

    public async Task LoadAsync(Stream stream)
    {
        try
        {
            using var memoryStream = Streams.RentStream();
            await stream.CopyToAsync(memoryStream);
            File = memoryStream.ToArray();
            FileSelected = true;
            memoryStream.Position = 0;
            Image?.Dispose();
            Image = new Bitmap(memoryStream);
        }
        catch (Exception)
        {
            Dispose();
        }
    }

    [RelayCommand]
    public async Task PasteAsync(Control control)
    {
        if (TopLevel.GetTopLevel(control) is not { Clipboard: { } clipboard })
            return;
        if (await clipboard.TryGetBitmapAsync() is { } bitmap)
            try
            {
                Image?.Dispose();
                Image = bitmap;
                using var memoryStream = Streams.RentStream();
                bitmap.Save(memoryStream);
                File = memoryStream.ToArray();
                FileSelected = true;
            }
            catch (Exception)
            {
                Dispose();
            }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        FileSelected = false;
        File = ReadOnlyMemory<byte>.Empty;
        Image?.Dispose();
    }
}
