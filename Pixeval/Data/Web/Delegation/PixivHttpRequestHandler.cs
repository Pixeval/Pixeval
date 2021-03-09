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

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;

namespace Pixeval.Data.Web.Delegation
{
    public class PixivHttpRequestHandler : IHttpRequestHandler
    {
        public static readonly PixivHttpRequestHandler Instance = new PixivHttpRequestHandler();

        protected PixivHttpRequestHandler()
        {
        }

        public virtual void Handle(HttpRequestMessage httpRequestMessage)
        {
            switch (httpRequestMessage.RequestUri.IdnHost)
            {
                case "app-api.pixiv.net":
                    if (Session.Current.AccessToken.IsNullOrEmpty())
                    {
                        throw new TokenNotFoundException($"{nameof(Session.Current.AccessToken)} is empty");
                    }

                    httpRequestMessage.Headers.Authorization = null;
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.Current.AccessToken);

                    break;
                case var x when x == "pixiv.net" || x == "www.pixiv.net":
                    httpRequestMessage.Headers.TryAddWithoutValidation("Cookie", Session.Current.Cookie);
                    break;
            }

            if (httpRequestMessage.RequestUri.DnsSafeHost == "i.pximg.net" && !Settings.Global.MirrorServer.IsNullOrEmpty())
            {
                httpRequestMessage.RequestUri = new Uri(httpRequestMessage.RequestUri.ToString().Replace("i.pximg.net", Settings.Global.MirrorServer));
            }

            if (!httpRequestMessage.Headers.Contains("Accept-Language"))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("Accept-Language", AkaI18N.GetCultureAcceptLanguage());
            }
        }
    }
}