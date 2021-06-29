using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi
{
    [PublicAPI]
    public class MakoException : Exception
    {
        public MakoException()
        {
        }

        protected MakoException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MakoException([CanBeNull] string? message) : base(message)
        {
        }

        public MakoException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException)
        {
        }
    }
    
    [PublicAPI]
    public class MakoNetworkException : MakoException
    {
        public string Url { get; set; }
        
        public bool Bypass { get; set; }
        public int StatusCode { get; }

        public MakoNetworkException(string url, bool bypass, string? extraMsg, int statusCode)
            : base($"Network error while requesting URL: {url}:\n {extraMsg}\n Bypassing: {bypass}\n Status code: {statusCode}")
        {
            Url = url;
            Bypass = bypass;
            StatusCode = statusCode;
        }

        // We use Task<Exception> instead of Task<MakoNetworkException> to compromise with the generic variance
        public static async Task<Exception> FromHttpResponseMessage(HttpResponseMessage message, bool bypass)
        {
            return new MakoNetworkException(message.RequestMessage?.RequestUri?.ToString() ?? string.Empty, bypass, await message.Content.ReadAsStringAsync(), (int) message.StatusCode);
        }
    }
    
    [PublicAPI]
    public class MangaPagesNotFoundException : MakoException
    {
        public MangaPagesNotFoundException(Illustration illustration)
        {
            Illustration = illustration;
        }

        protected MangaPagesNotFoundException([NotNull] SerializationInfo info, StreamingContext context, Illustration illustration) : base(info, context)
        {
            Illustration = illustration;
        }

        public MangaPagesNotFoundException([CanBeNull] string? message, Illustration illustration) : base(message)
        {
            Illustration = illustration;
        }

        public MangaPagesNotFoundException([CanBeNull] string? message, [CanBeNull] Exception? innerException, Illustration illustration) : base(message, innerException)
        {
            Illustration = illustration;
        }

        public Illustration Illustration { get; }
    }

    /// <summary>
    /// 搜索榜单时设定的日期大于等于当前日期-2天
    /// </summary>
    [PublicAPI]
    public class RankingDateOutOfRangeException : MakoException
    {
        public RankingDateOutOfRangeException()
        {
        }

        protected RankingDateOutOfRangeException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RankingDateOutOfRangeException([CanBeNull] string? message) : base(message)
        {
        }

        public RankingDateOutOfRangeException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// When a <see cref="PrivacyPolicy"/> is set to <see cref="PrivacyPolicy.Private"/> while the uid is not equivalent to the <see cref="MakoClient.Session"/>
    /// </summary>
    [PublicAPI]
    public class IllegalPrivatePolicyException : MakoException
    {
        public string Uid { get; }

        public IllegalPrivatePolicyException(string uid)
        {
            Uid = uid;
        }

        protected IllegalPrivatePolicyException([NotNull] SerializationInfo info, StreamingContext context, string uid) : base(info, context)
        {
            Uid = uid;
        }

        public IllegalPrivatePolicyException([CanBeNull] string? message, string uid) : base(message)
        {
            Uid = uid;
        }

        public IllegalPrivatePolicyException([CanBeNull] string? message, [CanBeNull] Exception? innerException, string uid) : base(message, innerException)
        {
            Uid = uid;
        }
    }

    /// <summary>
    /// Raised if you're trying to set the sort option to popular_desc without a premium access
    /// </summary>
    [PublicAPI]
    public class IllegalSortOptionException : MakoException
    {
        public IllegalSortOptionException()
        {
        }

        protected IllegalSortOptionException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IllegalSortOptionException([CanBeNull] string? message) : base(message)
        {
        }

        public IllegalSortOptionException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException)
        {
        }
    }
}