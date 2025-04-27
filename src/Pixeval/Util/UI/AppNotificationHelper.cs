// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Windows.Foundation;

namespace Pixeval.Util.UI;

public static class AppNotificationHelper
{
    public static void ShowTextAppNotification(string title, string content, Uri? logoUri = null, Action<AppNotificationBuilder>? contentBuilder = null)
    {
        if (!AppNotificationManager.IsSupported())
            return;
        var builder = new AppNotificationBuilder()
            .AddText(title)
            .AddText(content);
        contentBuilder?.Invoke(builder);
        if (logoUri is not null)
        {
            _ = builder.SetAppLogoOverride(logoUri, AppNotificationImageCrop.Default);
        }

        var appNotification = builder.BuildNotification();
        AppNotificationManager.Default.Show(appNotification);
    }

    public static ProgressAppNotification WithProgress(string title, string subtitle, string progressBarText, Uri logoUri)
    {
        var notification = new ProgressAppNotification(title, subtitle, progressBarText, logoUri);
        notification.Send();
        return notification;
    }

    /// <summary>
    /// TODO Not running
    /// </summary>
    public record ProgressAppNotification(string Title, string Subtitle, string ProgressBarText, Uri LogoUri)
    {
        private readonly string _tag = Guid.NewGuid().ToString();
        private uint _updateCounter;

        public void Send()
        {
            var content = new AppNotificationBuilder()
                .AddText(Title)
                .SetTag(_tag)
                .SetAppLogoOverride(LogoUri)
                .AddProgressBar(new()
                {
                    Title = Subtitle,
                    Status = ProgressBarText,
                    Value = 0,
                    ValueStringOverride = "0"
                })
                .BuildNotification();
            AppNotificationManager.Default.Show(content);
        }

        public IAsyncOperation<AppNotificationProgressResult> UpdateAsync(double progress)
        {
            _updateCounter++;
            return AppNotificationManager.Default.UpdateAsync(
                new(_updateCounter)
                {
                    Title = Subtitle + _updateCounter,
                    Status = ProgressBarText + _updateCounter,
                    Value = progress,
                    ValueStringOverride = progress.ToString()
                }, _tag);
        }
    }
}
