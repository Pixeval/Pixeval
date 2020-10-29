#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserActivities;
using Windows.UI.Shell;
using AdaptiveCards;
using Newtonsoft.Json;
using Pixeval.Data.ViewModel;
using Pixeval.Objects.Generic;

namespace Pixeval.Core
{
    /// <summary>
    ///     Provides a set of functions to create a Windows 10 Timeline Activity,
    ///     for more information and underlying implementation, see
    ///     <a href="https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/modernize-wpf-tutorial-4" />
    /// </summary>
    public class WindowsUserActivityManager : ITimelineService
    {
        public static readonly WindowsUserActivityManager GlobalLifeTimeScope = new WindowsUserActivityManager();
        private readonly Uri iconUri = new Uri("http://qa23pqcql.bkt.clouddn.com/pxlogo.ico");
        private UserActivitySession userActivitySession;

        public bool VerifyRationality(BrowsingHistory browsingHistory)
        {
            return true;
        }

        public async void Insert(BrowsingHistory browsingHistory)
        {
            var userActivityChannel = UserActivityChannel.GetDefault();
            var model = await GetPixevalTimelineModel(browsingHistory);
            var userActivity = await userActivityChannel.GetOrCreateUserActivityAsync($"Pixeval-{model.Id}-{DateTime.Now:s}");
            userActivity.VisualElements.DisplayText = model.Title;
            userActivity.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(BuildAdaptiveCard(model));
            userActivity.VisualElements.Attribution = new UserActivityAttribution(iconUri);
            userActivity.VisualElements.AttributionDisplayText = "Pixeval";
            userActivity.ActivationUri = new Uri(browsingHistory.Type switch
            {
                "illust"    => $"pixeval://www.pixiv.net/artworks/{model.Id}",
                "user"      => $"pixeval://www.pixiv.net/users/{model.Id}",
                "spotlight" => $"pixeval://www.pixivision.net/en/a/{model.Id}",
                _           => throw new ArgumentException(nameof(browsingHistory.Type))
            });
            await userActivity.SaveAsync();
            userActivitySession?.Dispose();
            userActivitySession = userActivity.CreateSession();
        }

        private static async Task<PixevalTimelineModel> GetPixevalTimelineModel(BrowsingHistory history)
        {
            var p = new PixevalTimelineModel { Background = await PixivIO.GetResizedBase64UriOfImageFromUrl(history.BrowseObjectThumbnail), Id = history.BrowseObjectId, Title = history.BrowseObjectState };
            switch (history.Type)
            {
                case "illust":
                    p.Author = history.IllustratorName;
                    return p;
                case "user":
                case "spotlight": return p;
            }

            throw new ArgumentException(nameof(history.Type));
        }

        private static string BuildAdaptiveCard(PixevalTimelineModel pixevalTimelineModel)
        {
            var card = new StringifyBackgroundAdaptiveCard("1.0") { StringifyUrl = pixevalTimelineModel.Background };
            card.Body.Add(new AdaptiveTextBlock
            {
                Text = pixevalTimelineModel.Title,
                Weight = AdaptiveTextWeight.Bolder,
                Wrap = true,
                Size = AdaptiveTextSize.Large,
                MaxLines = 3
            });
            if (!pixevalTimelineModel.Author.IsNullOrEmpty())
            {
                card.Body.Add(new AdaptiveTextBlock
                {
                    Text = pixevalTimelineModel.Author,
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true,
                    Size = AdaptiveTextSize.Small,
                    MaxLines = 3,
                    Spacing = AdaptiveSpacing.Small
                });
            }

            return card.ToJson();
        }

        private class PixevalTimelineModel
        {
            public string Id { get; set; }

            public string Title { get; set; }

            public string Author { get; set; }

            public string Background { get; set; }
        }

        // https://stackoverflow.com/questions/55663963/adaptive-cards-serving-images-in-bytes
        private class StringifyBackgroundAdaptiveCard : AdaptiveCard
        {
            public StringifyBackgroundAdaptiveCard(AdaptiveSchemaVersion schemaVersion) : base(schemaVersion)
            {
            }

            public StringifyBackgroundAdaptiveCard(string schemaVersion) : base(schemaVersion)
            {
            }

            [JsonProperty("backgroundImage")]
            public string StringifyUrl { get; set; }
        }
    }
}