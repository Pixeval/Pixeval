// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Threading.Tasks;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using WebApiClientCore.Attributes;
using WebApiClientCore.Parameters;

namespace Pixeval.CoreApi.Net.EndPoints;

[HttpHost("https://saucenao.com/")]
public interface IReverseSearchApiEndPoint
{
    [HttpPost("/search.php")]
    Task<ReverseSearchResponse> GetSauceAsync([FormDataContent] FormDataFile file, [PathQuery] ReverseSearchRequest request);
}
