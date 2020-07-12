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

using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pixeval.Interchange
{
    public class Program
    {
        private const string IllustRegex = "pixeval://(www\\.)?pixiv\\.net/artworks/(?<id>\\d+)";
        private const string UserRegex = "pixeval://(www\\.)?pixiv\\.net/users/(?<id>\\d+)";
        private const string SpotlightRegex = "pixeval://(www\\.)?pixivision\\.net/\\w{2}/a/(?<id>\\d+)";

        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task Main(string[] args)
        {
            using (HttpClient)
            {
                if (!args.Any()) return;
                var url = args[0];
                // check protocol rationality
                if (!(Regex.IsMatch(url, IllustRegex) ||
                    Regex.IsMatch(url, UserRegex) ||
                    Regex.IsMatch(url, SpotlightRegex)))
                    return;
                // check if there's an instance is running
                if (PixevalInstanceRunning())
                {
                    // send Pixeval custom pluggable protocol if so
                    await HttpClient.PostAsync("http://127.0.0.1:12547/open",
                                               new StringContent(url, Encoding.UTF8));
                }
                else
                {
                    // otherwise execute Pixeval and pass the protocol as argument
                    Process.Start("Pixeval", url);
                }
            }
        }
        private static bool PixevalInstanceRunning()
        {
            return Process.GetProcessesByName("Pixeval").Length > 0;
        }
    }
}