// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels.Viewers;

public partial class CommentItemViewModel(Comment comment) : ObservableObject, IFactory<Comment, CommentItemViewModel>, IDisposable
{
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

    public long Id => Comment.Id;

    [ObservableProperty]
    public partial IImage? AvatarSource { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepliesCount))]
    public partial ObservableCollection<CommentItemViewModel>? Replies { get; set; }

    public string? RepliesCount => Replies?.Count.ToString();

    public async Task LoadRepliesAsync(SimpleWorkType entryType)
    {
        if (HasReplies)
            Replies = new ObservableCollection<CommentItemViewModel>(
                await (entryType switch
                {
                    SimpleWorkType.IllustrationAndManga => App.AppViewModel.MakoClient.IllustrationCommentReplies(Id),
                    SimpleWorkType.Novel => App.AppViewModel.MakoClient.NovelCommentReplies(Id),
                    _ => throw new ArgumentOutOfRangeException(nameof(entryType))
                }).Select(c => new CommentItemViewModel(c)).ToListAsync());
        else
            Replies = null;
    }

    public async Task LoadAvatarSource()
    {
        AvatarSource = await CacheHelper.GetBitmapFromCacheAsync(Comment.CommentPoster.ProfileImageUrls.Medium);
    }

    public void AddComment(Comment comment)
    {
        Replies ??= [];
        Replies.Insert(0, new CommentItemViewModel(comment));
    }

    public void DeleteComment(CommentItemViewModel viewModel)
    {
        _ = Replies?.Remove(viewModel);
        if (Replies is { Count: 0 })
            Replies = null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Replies?.ForEach(r => r.Dispose());
    }

    public static CommentItemViewModel CreateInstance(Comment entry) => new(entry);
}
