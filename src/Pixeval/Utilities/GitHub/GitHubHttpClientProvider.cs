// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Net.Http;
using Pixeval.AppManagement;

namespace Pixeval.Utilities.GitHub;

public sealed class GitHubHttpClientProvider(NetworkSettingsGroup networkSettings) : IDisposable
{
    public HttpClient Client { get; private set; } = GitHubDirectHttpClientFactory.Create(networkSettings);

    public void Reset()
    {
        var client = Client;
        Client = GitHubDirectHttpClientFactory.Create(networkSettings);
        client.Dispose();
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}
