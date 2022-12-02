#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/MakoNetworkException.cs
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

namespace Pixeval.CoreApi.Exceptions;

public class MakoNetworkException : MakoException
{
    public MakoNetworkException(string url, bool bypass, string? extraMsg, int statusCode)
        : base($"Network error while requesting URL: {url}:\n {extraMsg}\n Bypassing: {bypass}\n Status code: {statusCode}")
    {
        Url = url;
        Bypass = bypass;
        StatusCode = statusCode;
    }

    public string Url { get; set; }

    public bool Bypass { get; set; }
    public int StatusCode { get; }

    // We use Task<Exception> instead of Task<MakoNetworkException> to compromise with the generic variance
    public static async Task<System.Exception> FromHttpResponseMessageAsync(HttpResponseMessage message, bool bypass)
    {
        return new MakoNetworkException(message.RequestMessage?.RequestUri?.ToString() ?? string.Empty, bypass, await message.Content.ReadAsStringAsync().ConfigureAwait(false), (int)message.StatusCode);
    }
}