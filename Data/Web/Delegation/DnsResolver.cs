// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Data.Web.Protocol;
using Pixeval.Data.Web.Request;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Refit;

namespace Pixeval.Data.Web.Delegation
{
    public abstract class DnsResolver
    {
        public static readonly ThreadLocal<Dictionary<string, List<IPAddress>>> DnsCache = new ThreadLocal<Dictionary<string, List<IPAddress>>>(() => new Dictionary<string, List<IPAddress>>());

        private static bool _dnsQueryFailed;

        protected readonly ISet<IPAddress> IpList = new HashSet<IPAddress>(new IPAddressEqualityComparer());

        protected async Task<DnsResolveResponse> GetDnsJson(string hostname)
        {
            return await RestService.For<IResolveDnsProtocol>(new HttpClient(new HttpClientHandler {SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls}) {BaseAddress = new Uri(ProtocolBase.DnsServer), Timeout = TimeSpan.FromSeconds(5)})
                .ResolveDns(new DnsResolveRequest
                {
                    Ct = "application/dns-json",
                    Cd = "false",
                    Do = "false",
                    Name = hostname,
                    Type = "A"
                });
        }

        public async Task<IReadOnlyList<IPAddress>> Lookup(string hostname)
        {
            if (DnsCache.Value.ContainsKey(hostname)) return DnsCache.Value[hostname].ToImmutableList();

            if (_dnsQueryFailed) return CacheDefaultDns(hostname);

            DnsResolveResponse response;
            try
            {
                response = await GetDnsJson(hostname);
            }
            catch (Exception)
            {
                _dnsQueryFailed = true;
                return CacheDefaultDns(hostname);
            }

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
                    if (IpList.IsNullOrEmpty()) IpList.AddRange(UseDefaultDns());
                }

                CacheDns(hostname);
                return IpList.ToImmutableList();
            }

            IpList.AddRange(UseDefaultDns());
            CacheDns(hostname);
            return IpList.ToImmutableList();
        }

        private IReadOnlyList<IPAddress> CacheDefaultDns(string hostname)
        {
            IpList.AddRange(UseDefaultDns());
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

        protected abstract IEnumerable<IPAddress> UseDefaultDns();

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