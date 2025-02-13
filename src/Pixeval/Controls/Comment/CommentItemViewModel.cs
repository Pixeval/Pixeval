// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO.Caching;
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
        AvatarSource = await CacheHelper.GetSourceFromCacheAsync(Comment.CommentPoster.ProfileImageUrls.Medium);
    }

    public void AddComment(Comment comment)
    {
        Replies ??= [];

        Replies.Insert(0, new CommentItemViewModel(comment, EntryType, EntryId));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Replies?.ForEach(r => r.Dispose());
    }
}
