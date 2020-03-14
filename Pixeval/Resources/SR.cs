using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pixeval.Models;
using Pixeval.Persisting;

namespace Pixeval.Resources
{
    // ReSharper disable once InconsistentNaming
    internal sealed class SR
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
        public const string AddedToDownloadList = "成功添加到下载列表";
        public const string AddedAllToDownloadList = "成功添加所有作品到下载列表";
        public const string ContentCopiedToClipboard = "已复制到剪贴板";
        public static readonly string AllDownloadComplete = $"已经下载所有图片到{Settings.Global.DownloadLocation}";

        public static string DownloadComplete(Illustration illustration)
        {
            return $"{illustration.Id}已经下载到{Settings.Global.DownloadLocation}";
        }

        public static string DownloadSpotlightComplete(SpotlightArticle spotlight)
        {
            return $"{spotlight.Id}已经下载到{Path.Combine(Settings.Global.DownloadLocation, "Spotlight")}";
        }

        public static string InputIllegal(string s)
        {
            return $"搜索{s}时必须输入纯数字哟~";
        }
    }
}
