#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/PixivApiNameResolver.cs
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

using System.Net;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Net;

public class PixivApiNameResolver : INameResolver
{
    public Task<IPAddress[]> Lookup(string hostname)
    {
        // not a good idea
        if (hostname.Contains("pixiv"))
        {
            return Task.FromResult(new[]
            {
                IPAddress.Parse("210.140.131.219"),
                IPAddress.Parse("210.140.131.223"),
                IPAddress.Parse("210.140.131.226")
            });
        }

        return Dns.GetHostAddressesAsync(hostname);
    }
}