#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/ToastNotificationHelper.cs
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
using System.Reflection;
using Windows.UI.Notifications;
using CommunityToolkit.WinUI.Notifications;
using Pixeval.Utilities;

namespace Pixeval.Util.UI;

public static class ToastNotificationHelper
{
    public record ProgressToastNotification(string Title, string Subtitle, string ProgressBarText, Uri LogoUri)
    {
        private uint _updateCounter;
        private readonly string _tag = Guid.NewGuid().ToString();

        public void Send()
        {
            var content = new ToastContentBuilder()
                .AddText(Title)
                .AddAppLogoOverride(LogoUri)
                .AddVisualChild(new AdaptiveProgressBar
                {
                    Title = Subtitle,
                    Status = ProgressBarText,
                    Value = new BindableProgressBarValue("progressValue")
                })
                .GetToastContent();
            var toast = new ToastNotification(content.GetXml())
            {
                Tag = _tag,
                ExpiresOnReboot = true,
                Data = new NotificationData
                {
                    SequenceNumber = _updateCounter,
                    Values =
                    {
                        ["progressValue"] = "0.0"
                    }
                }
            };
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void Update(double progress)
        {
            var data = new NotificationData
            {
                SequenceNumber = _updateCounter++,
                Values =
                {
                    ["progressValue"] = $"{progress.Normalize(100, 0):#.#}"
                }
            };
            ToastNotificationManager.CreateToastNotifier().Update(data, _tag);
        }
    }

    private static readonly PropertyInfo AppLogoOverrideUriProperty = typeof(ToastContentBuilder).GetProperty("AppLogoOverrideUri", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public static ToastContentBuilder AddInlineImage(
        this ToastContentBuilder builder,
        string uri,
        string? alternateText = default,
        bool? addImageQuery = default,
        AdaptiveImageCrop? hintCrop = default)
    {
        var inlineImage = new AdaptiveImage
        {
            Source = uri
        };

        if (hintCrop != null)
        {
            inlineImage.HintCrop = hintCrop.Value;
        }

        if (alternateText != default)
        {
            inlineImage.AlternateText = alternateText;
        }

        if (addImageQuery != default)
        {
            inlineImage.AddImageQuery = addImageQuery;
        }

        return builder.AddVisualChild(inlineImage);
    }

    public static ToastContentBuilder AddAppLogoOverride(
        this ToastContentBuilder builder,
        string uri,
        ToastGenericAppLogoCrop? hintCrop = default,
        string? alternateText = default,
        bool? addImageQuery = default)
    {
        var appLogoOverrideUri = new ToastGenericAppLogo
        {
            Source = uri
        };

        if (hintCrop is { } crop)
        {
            appLogoOverrideUri.HintCrop = crop;
        }

        if (alternateText is { } alt)
        {
            appLogoOverrideUri.AlternateText = alt;
        }

        if (addImageQuery is { } query)
        {
            appLogoOverrideUri.AddImageQuery = query;
        }

        AppLogoOverrideUriProperty.SetValue(builder, appLogoOverrideUri);

        return builder;
    }

    public static void ShowTextToastNotification(string title, string content, string? logoUri = null, Action<ToastContentBuilder>? contentBuilder = null)
    {
        var builder = new ToastContentBuilder()
            .SetBackgroundActivation()
            .AddText(title, AdaptiveTextStyle.Header)
            .AddText(content, AdaptiveTextStyle.Caption);
        contentBuilder?.Invoke(builder);
        if (logoUri is not null)
        {
            builder.AddAppLogoOverride(logoUri, ToastGenericAppLogoCrop.Default);
        }

        builder.Show();
    }

    public static ProgressToastNotification WithProgress(string title, string subtitle, string progressBarText, Uri logoUri)
    {
        var notification = new ProgressToastNotification(title, subtitle, progressBarText, logoUri);
        notification.Send();
        return notification;
    }
}