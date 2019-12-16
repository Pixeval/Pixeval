using System.IO;
using Pixeval.Data.ViewModel;
using Pixeval.Persisting;

namespace Pixeval.Objects.Exceptions
{
    public sealed class Externally
    {
        public const string EmailOrPasswordIsWrong = "用户名或密码错误";

        public const string EmptyEmailOrPasswordIsNotAllowed = "用户名或密码不能为空";

        public const string SuccessfullyFollowUser = "成功关注用户";

        public const string SuccessfullyUnFollowUser = "成功取消关注";

        public const string IdDoNotExists = "您所输入的ID不存在, 请检查后再试一次吧~";

        public const string CannotFindUser = "抱歉, Pixeval找不到您搜索的用户呢, 请仔细检查用户ID后再来一次吧~";

        public const string InputIsEmpty = "请在输入搜索关键字后再进行搜索~";

        public const string QueryNotResponding = "抱歉, Pixeval无法根据当前的设置找到任何作品, 或许是您的页码设置过大/关键字不存在/没有收藏任何作品, 请检查后再尝试吧~";

        public const string ClearedDownloadList = "成功清空下载列表";

        public const string AddedAllToDownloadList = "成功添加到下载列表";

        public static readonly string AllDownloadComplete = $"已经下载所有图片到{Settings.Global.DownloadLocation}";

        public static string DownloadComplete(Illustration illustration)
        {
            return $"{illustration.Id}已经下载到{Settings.Global.DownloadLocation}";
        }

        public static string DownloadSpotlightComplete(SpotlightArticle spotlight)
        {
            return $"{spotlight.Id}已经下载到{Path.Combine(Settings.Global.DownloadLocation, "Spotlight")}";
        }

        public static string NoticeProgressString(int pages)
        {
            return $"正在为您查找第{Settings.Global.QueryStart}到第{Settings.Global.QueryPages + Settings.Global.QueryStart - 1}页, 您所查找的关键字总共有{pages}页";
        }

        public static string InputIllegal(string s)
        {
            return $"搜索{s}时必须输入纯数字哟~";
        }
    }
}