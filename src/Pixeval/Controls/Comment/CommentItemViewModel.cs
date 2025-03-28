// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Util.IO.Caching;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls;

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
    public partial ImageSource AvatarSource { get; set; } = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepliesIsNotNull), nameof(RepliesCount))]
    public partial ObservableCollection<CommentItemViewModel>? Replies { get; set; }

    public bool RepliesIsNotNull => Replies is not null;

    public string? RepliesCount => Replies?.Count.ToString();

    public async Task LoadRepliesAsync(SimpleWorkType entryType)
    {
        if (HasReplies)
            Replies = await
                (entryType switch
                {
                    SimpleWorkType.IllustAndManga => App.AppViewModel.MakoClient.IllustrationCommentReplies(Id),
                    SimpleWorkType.Novel => App.AppViewModel.MakoClient.NovelCommentReplies(Id),
                    _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, IFetchEngine<Comment>>(entryType)
                }).Select(c => new CommentItemViewModel(c))
                .ToObservableCollectionAsync();
        else
            Replies = null;
    }

    public async Task LoadAvatarSource()
    {
        AvatarSource = await CacheHelper.GetSourceFromCacheAsync(Comment.CommentPoster.ProfileImageUrls.Medium);
    }

    public void AddComment(Comment comment)
    {
        Replies ??= [];

        Replies.Insert(0, new CommentItemViewModel(comment));
        OnPropertyChanged(nameof(RepliesCount));
    }

    public void DeleteComment(CommentItemViewModel viewModel)
    {
        _ = Replies?.Remove(viewModel);
        OnPropertyChanged(nameof(RepliesCount));
        if (Replies is { Count: 0 })
            Replies = null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Replies?.ForEach(r => r.Dispose());
    }

    public static CommentItemViewModel CreateInstance(Comment entry) => new CommentItemViewModel(entry);
}
