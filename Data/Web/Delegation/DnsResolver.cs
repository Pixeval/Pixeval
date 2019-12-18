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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Data.Web.Protocol;
using Pixeval.Data.Web.Request;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Refit;

namespace Pixeval.Data.Web.Delegation
{
    /// <summary>
    ///     Use a free <a href="https://1.0.0.1">DNS server</a> as DNS resolver instead of system default to avoid DNS
    ///     pollution, thanks to <a href="https://github.com/Notsfsssf">@Notsfsssf</a>'s idea
    /// </summary>
    public abstract class DnsResolver
    {
        public static readonly ThreadLocal<Dictionary<string, List<IPAddress>>> DnsCache = new ThreadLocal<Dictionary<string, List<IPAddress>>>(() => new Dictionary<string, List<IPAddress>>());

        protected readonly ISet<IPAddress> IpList = new HashSet<IPAddress>(new IPAddressEqualityComparer());

        protected async Task<DnsResolveResponse> GetDnsJson(string hostname)
        {
            return await RestService.For<IResolveDnsProtocol>(ProtocolBase.DnsServer)
                .ResolveDns(new DnsResolveRequest
                {
                    Ct = "application/dns-json",
                    Cd = "false",
                    Do = "false",
                    Name = hostname,
                    Type = "A"
                });
        }

        /// <summary>
        ///     query dns according to specified <see cref="hostname" />
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns>the result of dns query</returns>
        public async Task<IList<IPAddress>> Lookup(string hostname)
        {
            if (DnsCache.Value.ContainsKey(hostname)) return DnsCache.Value[hostname].ToImmutableList();

            var response = await GetDnsJson(hostname);

            if (response != null)
            {
                var answer = response.Answers;
                if (!answer.IsNullOrEmpty())
                {
                    foreach (var queriedIp in answer)
                        if (IPAddress.TryParse(queriedIp.Data, out var address))
                            IpList.Add(address);
                }
                else
                {
                    IpList.AddRange(Dns.GetHostAddresses(hostname));
                    if (IpList.IsNullOrEmpty()) UseDefaultDns();
                }

                CacheDns(hostname);
                return IpList.ToImmutableList();
            }

            UseDefaultDns();
            CacheDns(hostname);
            return IpList.ToImmutableList();
        }

        private void CacheDns(string hostname)
        {
            if (DnsCache.Value.ContainsKey(hostname))
                DnsCache.Value[hostname].AddRange(IpList);
            else
                DnsCache.Value[hostname] = new List<IPAddress>(IpList);
        }

        protected abstract void UseDefaultDns();

        private class IPAddressEqualityComparer : IEqualityComparer<IPAddress>
        {
            public bool Equals(IPAddress x, IPAddress y)
            {
                if (x == null || y == null) return false;
                return x.ToString() == y.ToString();
            }

            public int GetHashCode(IPAddress obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}