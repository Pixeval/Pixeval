using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Misaki;
using Pixeval.I18N;
using Pixeval.Models.SauceNao;
using Pixeval.Utilities;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Work;

public partial class ArtworkSauceNaoSearchResultPage : ContentPage
{
    public ArtworkSauceNaoSearchResultPage(string apiKey, ReadOnlyMemory<byte> file)
    {
        InitializeComponent();
        _ = PostAsync(apiKey, file);
    }

    public async Task PostAsync(string apiKey, ReadOnlyMemory<byte> file)
    {
        var results = GetResults(apiKey, file);
        WorkContainer.ResetEngine(results);
    }

    public async IAsyncEnumerable<IArtworkInfo> GetResults(string apiKey, ReadOnlyMemory<byte> file)
    {
        var viewContainer = TopLevel.GetTopLevel(this)?.ViewContainer;

        IReadOnlyList<SauceNaoResult>? sauceNaoResults = null;
        try
        {
            var httpClient = App.AppViewModel.GetRequiredHttpClient();
            using var form = new MultipartFormDataContent();
            using var fileContent = new ReadOnlyMemoryContent(file);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "file", "img");
            var response = await httpClient.PostAsync(new SauceNaoRequest(apiKey).ToQueryString(), form);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync(SauceNaoResponseSerializerContext.Default.SauceNaoResponse);
            if (result is { Header.Status : 0, Results: { } results })
                sauceNaoResults = results;
            else
                viewContainer?.ShowError(I18NManager.GetResource(MiscResources.ExceptionEncountered), result?.Header.Message);
        }
        catch (Exception e)
        {
            viewContainer?.ShowError(I18NManager.GetResource(MiscResources.ExceptionEncountered), e.Message);
        }

        if (sauceNaoResults is not null)
            foreach (var result in sauceNaoResults)
                if (result.Data.ToIdentityInfo() is { } identityInfo
                    && await identityInfo.TryGetArtworkInfoAsync() is { } artwork)
                    yield return artwork;
    }
}
