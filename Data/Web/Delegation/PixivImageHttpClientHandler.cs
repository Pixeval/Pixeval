// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Net.Http;

namespace Pixeval.Data.Web.Delegation
{
    public class PixivImageHttpClientHandler : DnsResolvedHttpClientHandler
    {
        public static HttpMessageHandler Instance = new PixivImageHttpClientHandler();

        private PixivImageHttpClientHandler() : base(PixivAuthenticationHttpRequestHandler.Instance)
        {
        }

        protected override DnsResolver DnsResolver { get; set; } = PixivImageDnsResolver.Instance;
    }
}