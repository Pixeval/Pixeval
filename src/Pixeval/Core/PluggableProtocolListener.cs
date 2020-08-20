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

using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Pixeval.Wpf.Objects.Primitive;
using Pixeval.Wpf.View;

namespace Pixeval.Wpf.Core
{
    public class TransferController : WebApiController
    {
        [Route(HttpVerbs.Post, "/open")]
        public async Task DoOpen()
        {
            MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.GlobalActivate());
            await PluggableProtocolParser.Parse(await HttpContext.GetRequestBodyAsStringAsync());
        }
    }

    public class PluggableProtocolListener
    {
        private static WebServer _server;

        public static void StartServer()
        {
            _server = new WebServer(o => o.WithUrlPrefix("http://127.0.0.1:12547").WithMode(HttpListenerMode.Microsoft))
                .WithWebApi("/", m => m.WithController<TransferController>());
            _server.Start();
        }
    }
}
