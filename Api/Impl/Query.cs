using System.Collections.Generic;
using System.Threading.Tasks;
using Pzxlane.Api.Supplier;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;

namespace Pzxlane.Api.Impl
{
    public sealed class Query : IIterableContentSupplier<QueryResponse.Response, QueryResponse>
    {
        private readonly string tag;

        public Query(string tag, int start, int end)
        {
            Start = start;
            End = end;
            this.tag = tag;
        }


        public string GetIllustId(QueryResponse.Response entity)
        {
            return entity.Id.ToString();
        }

        public async Task<IEnumerable<QueryResponse.Response>> GetIllusts(object param)
        {
            Context = await HttpClientFactory.PublicApiService.QueryWorks(new QueryWorksRequest {Tag = tag, Offset = (int) param});
            return Context.ToResponse;
        }

        public QueryResponse Context { get; private set; }

        public string Status => Context.Status;

        public int Start { get; }

        public int End { get; }
    }
}