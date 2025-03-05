// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Net.Http;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Global.Exception;

public class MakoNetworkException(string url, bool domainFronting, string? extraMsg, int statusCode)
    : MakoException($"Network error while requesting URL: {url} (Domain fronting: {domainFronting}, Status code: {statusCode}) {extraMsg}")
{
    public string Url { get; set; } = url;

    public bool DomainFronting { get; set; } = domainFronting;

    public int StatusCode { get; } = statusCode;

    /// <summary>
    /// We use Task&lt;Exception&gt; instead of Task&lt;MakoNetworkException&gt; to compromise with the generic variance
    /// </summary>
    /// <param name="message"></param>
    /// <param name="domainFronting"></param>
    /// <returns></returns>
    public static async Task<System.Exception> FromHttpResponseMessageAsync(HttpResponseMessage message, bool domainFronting)
    {
        return new MakoNetworkException(message.RequestMessage?.RequestUri?.ToString() ?? "", domainFronting, await message.Content.ReadAsStringAsync().ConfigureAwait(false), (int) message.StatusCode);
    }
}
