using Pixeval.CoreApi.Model;
using Refit;

namespace Pixeval.CoreApi.Net.Response
{
    public class PostCommentResponse
    {
        [AliasAs("comment")]
        public Comment? Comment { get; set; }
    }
}