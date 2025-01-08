// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Net.Http;

namespace Pixeval.CoreApi.Net;

internal class MakoHttpClient(HttpMessageHandler handler) : HttpClient(handler);
