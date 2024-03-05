#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
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

#endregion

using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Pixeval.Data.Web.Delegation
{
    public class PixivApiDnsResolver : DnsResolver
    {
        public static readonly DnsResolver Instance = new PixivApiDnsResolver();

        protected override IEnumerable<IPAddress> UseDefaultDns()
        {
            if (File.Exists("./ApiIP.cfg"))
            {
                foreach (var ip in File.ReadAllLines("./ApiIP.cfg"))
                {
                    yield return IPAddress.Parse(ip);
                }
            }
            else
            {
                yield return IPAddress.Parse("210.140.131.219");
                yield return IPAddress.Parse("210.140.131.223");
                yield return IPAddress.Parse("210.140.131.226");
            }
        }
    }

    public class PixivImageDnsResolver : DnsResolver
    {
        public static readonly DnsResolver Instance = new PixivImageDnsResolver();

        protected override IEnumerable<IPAddress> UseDefaultDns()
        {
            if (File.Exists("./ImageIP.cfg"))
            {
                foreach (var ip in File.ReadAllLines("./ImageIP.cfg"))
                {
                    yield return IPAddress.Parse(ip);
                }
            }
            else
            {
                yield return IPAddress.Parse("210.140.92.141");
                yield return IPAddress.Parse("210.140.92.142");
                yield return IPAddress.Parse("210.140.92.143");
            }
        }
    }
}
