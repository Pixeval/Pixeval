using Refit;

namespace Pixeval.CoreApi.Net.Request
{
    public record DeleteCommentRequest
    {
        public DeleteCommentRequest(string commentId)
        {
            CommentId = commentId;
        }

        [AliasAs("comment_id")]
        public string CommentId { get; }
    }
}