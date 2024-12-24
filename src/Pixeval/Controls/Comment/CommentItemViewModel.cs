#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/CommentItemViewModel.cs
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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class CommentItemViewModel(Comment comment, SimpleWorkType type, long entryId) : ObservableObject, IDisposable
{
    public long EntryId { get; } = entryId;

    public SimpleWorkType EntryType { get; } = type;

    public Comment Comment { get; } = comment;

    public bool HasReplies => Comment.HasReplies;

    [MemberNotNullWhen(true, nameof(StampSource))]
    public bool IsStamp => Comment.CommentStamp is not null;

    public string? StampSource => Comment.CommentStamp?.StampUrl;

    public DateTimeOffset PostDate => Comment.Date;

    public string Poster => Comment.CommentPoster.Name;

    public long PosterId => Comment.CommentPoster.Id;

    public string CommentContent => Comment.CommentContent;

    public bool IsMe => PosterId == App.AppViewModel.PixivUid;

    public long CommentId => Comment.Id;

    [ObservableProperty]
    public partial ImageSource AvatarSource { get; set; } = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepliesIsNotNull), nameof(RepliesCount))]
    public partial ObservableCollection<CommentItemViewModel>? Replies { get; set; }

    public bool RepliesIsNotNull => Replies is not null;

    public string? RepliesCount => Replies?.Count.ToString();

    public async Task LoadRepliesAsync()
    {
        Replies = await
            (EntryType switch
            {
                SimpleWorkType.IllustAndManga => App.AppViewModel.MakoClient.IllustrationCommentReplies(CommentId),
                SimpleWorkType.Novel => App.AppViewModel.MakoClient.NovelCommentReplies(CommentId),
                _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, IFetchEngine<Comment>>(EntryType)
            }).Select(c => new CommentItemViewModel(c, EntryType, EntryId))
            .ToObservableCollectionAsync();
    }

    public async Task LoadAvatarSource()
    {
        AvatarSource = await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>().GetSourceFromMemoryCacheAsync(Comment.CommentPoster.ProfileImageUrls.Medium);
    }

    public void AddComment(Comment comment)
    {
        Replies ??= [];

        Replies.Insert(0, new CommentItemViewModel(comment, EntryType, EntryId));
    }

    public async Task<Paragraph> GetReplyContentParagraphAsync()
    {
        var paragraph = new Paragraph();
        foreach (var replyContentToken in ReplyEmojiHelper.EnumerateTokens(CommentContent))
        {
            switch (replyContentToken)
            {
                case ReplyContentToken.TextToken(var content):
                    paragraph.Inlines.Add(new Run
                    {
                        Text = content
                    });
                    break;
                case ReplyContentToken.EmojiToken(var emoji):
                    paragraph.Inlines.Add(new InlineUIContainer
                    {
                        Child = new Image
                        {
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Source = await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>().GetSourceFromMemoryCacheAsync(emoji.GetReplyEmojiDownloadUrl(), desiredWidth: 14),
                            Width = 14,
                            Height = 14
                        }
                    });

                    break;
            }
        }

        return paragraph;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Replies?.ForEach(r => r.Dispose());
    }
}
