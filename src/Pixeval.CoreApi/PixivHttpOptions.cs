#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/PixivHttpOptions.cs
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

using Pixeval.CoreApi.Net;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.CoreApi;

public class PixivHttpOptions
{
    public string AppApiBaseUrl { get; set; }

    public string WebApiBaseUrl { get; set; }

    public string OAuthBaseUrl { get; set; }

    public string ImageHost { get; set; } = "i.pximg.net";

    public string WebApiHost { get; set; } = "www.pixiv.net"; // experiments revealed that the secondary domain 'www' is required 

    public string AppApiHost { get; set; } = "app-api.pixiv.net";

    public string OAuthHost { get; set; } = "oauth.secure.pixiv.net";

    public readonly Regex BypassRequiredHost = new("^app-api\\.pixiv\\.net$|^www\\.pixiv\\.net$");
    
}