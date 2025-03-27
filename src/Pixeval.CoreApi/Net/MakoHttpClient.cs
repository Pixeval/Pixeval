// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Net.Http;

namespace Mako.Net;

internal class MakoHttpClient(HttpMessageHandler handler) : HttpClient(handler);
