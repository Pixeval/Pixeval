using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeval.CoreApi.Net.Requests;
using Pixeval.CoreApi.Net.Responses;

namespace Pixeval.CoreApi.Services;

public interface IPixivReverseSearchService
{
    [Multipart]
    [Post("/search.php")]
    Task<ReverseSearchResponse> GetSauceAsync([Query] ReverseSearchRequest request, [AliasAs("file")] StreamPart file);
}