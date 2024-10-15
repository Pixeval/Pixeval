#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/MakoNetworkException.cs
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

using System.Net.Http;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Global.Exception;

public class MakoNetworkException(string url, bool domainFronting, string? extraMsg, int statusCode)
    : MakoException($"Network error while requesting URL: {url}(Domain fronting: {domainFronting}, Status code: {statusCode}) {extraMsg}")
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
        return new MakoNetworkException(message.RequestMessage?.RequestUri?.ToString() ?? "", domainFronting, await message.Content.ReadAsStringAsync().ConfigureAwait(false), (int)message.StatusCode);
    }
}
