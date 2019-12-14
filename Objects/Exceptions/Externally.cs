using System.IO;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Persisting;

namespace Pixeval.Objects.Exceptions
{
    public sealed class Externally
    {
        public const string EmailOrPasswordIsWrong = "用户名或密码错误";

        public const string EmptyEmailOrPasswordIsNotAllowed = "用户名或密码不能为空";

        public static string DownloadComplete(Illustration illustration) => $"{illustration.Id}已经下载到{Settings.Global.DownloadLocation}";

        public static string DownloadSpotlightComplete(SpotlightArticle spotlight) => $"{spotlight.Id}已经下载到{Path.Combine(Settings.Global.DownloadLocation, "Spotlight")}";

        public static readonly string AllDownloadComplete = $"已经下载所有图片到{Settings.Global.DownloadLocation}";

        public const string ClearedDownloadList = "成功清空下载列表";

        public const string AddedAllToDownloadList = "成功添加到下载列表";
    }
}