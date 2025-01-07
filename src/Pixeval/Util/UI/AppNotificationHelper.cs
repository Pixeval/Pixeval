#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ToastNotificationHelper.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using Windows.Foundation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace Pixeval.Util.UI;

public static class AppNotificationHelper
{
    public static void ShowTextAppNotification(string title, string content, Uri? logoUri = null, Action<AppNotificationBuilder>? contentBuilder = null)
    {
        if (!AppNotificationManager.IsSupported())
            return;
        var builder = new AppNotificationBuilder()
            .AddText(title)
            .AddText(content)
            .SetInlineImage(logoUri, AppNotificationImageCrop.Circle);
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
