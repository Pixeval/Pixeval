#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/MakoHttpClient.cs
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

using System;
using System.Net.Http;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Net;

internal class MakoHttpClient : HttpClient
{
    private MakoHttpClient(HttpMessageHandler handler) : base(handler)
    {
    }

    public static MakoHttpClient Create(HttpMessageHandler handler,
        Action<MakoHttpClient>? action = null)
    {
        var mako = new MakoHttpClient(handler);
        action?.Let(ac => ac!(mako));
        return mako;
    }
}