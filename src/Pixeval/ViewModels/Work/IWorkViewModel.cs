// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Misaki;
using Pixeval.Filters;

namespace Pixeval.ViewModels;

public interface IWorkViewModel
{
    bool IsBookmarkEnabled { get; set; }

    bool IsBookmarkSupported { get; }

    IArtworkInfo Entry { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.AddToBookmarkCommand"/>
    IAsyncRelayCommand<(IEnumerable<string> UserTags, bool IsPrivate, Control? Control)> AddToBookmarkCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.BookmarkCommand"/>
    IAsyncRelayCommand<Control?> BookmarkCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveCommand"/>
    IAsyncRelayCommand<Control?> SaveCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.TryLoadThumbnailAsync"/>
    ValueTask<bool> TryLoadThumbnailAsync(object key);

    /// <inheritdoc cref="WorkEntryViewModel{T}.UnloadThumbnail"/>
    void UnloadThumbnail(object key);

    bool Filter(TreeNodeBase node) => node switch
    {
        LeafSequence sequence => sequence switch
        {
            {
                IsNot: var isNot,
                Type: SequenceType.And,
                Children: { Count: > 0 } children
            } => isNot ^ children.Select(Filter).All(t => t),
            {
                IsNot: var isNot,
                Type: SequenceType.Or,
                Children: { Count: > 0 } children
            } => isNot ^ children.Select(Filter).Any(t => t),
            _ => true
        },
        QueryLeaf
        {
            IsNot: var isNot
        } q => isNot ^ FilterQuery(q),
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };

    bool FilterQuery(QueryLeaf queryToken)
    {
        return queryToken switch
        {
            StringLeaf stringLeaf => stringLeaf.Type switch
            {
                StringType.Title => StringCompare(stringLeaf.Content, Entry.Title),
                StringType.Author => Entry.Authors.Any(t => StringCompare(stringLeaf.Content, t.Name)),
                StringType.Tag => Entry.Tags.Any(t => t.Any(h => StringCompare(stringLeaf.Content, h.Name) || StringCompare(stringLeaf.Content, h.TranslatedName))),
                _ => throw new ArgumentOutOfRangeException(nameof(stringLeaf.Type))
            },
            BoolLeaf boolLeaf => boolLeaf.IsExclude ^ boolLeaf.Type switch
            {
                BoolType.R18 => Entry.SafeRating.IsR18,
                BoolType.R18G => Entry.SafeRating.IsR18G,
                BoolType.Ai => Entry.IsAiGenerated,
                BoolType.Gif => Entry.ImageType is ImageType.SingleAnimatedImage,
                _ => throw new ArgumentOutOfRangeException(nameof(boolLeaf.Type))
            },
            NumericRangeLeaf numericRangeLeaf => numericRangeLeaf.Type switch
            {
                NumericRangeType.Bookmark => numericRangeLeaf.IsInRange(Entry.TotalFavorite),
                NumericRangeType.Index => true,
                _ => throw new ArgumentOutOfRangeException(nameof(numericRangeLeaf.Type))
            },
            FloatRangeLeaf floatRangeLeaf => floatRangeLeaf.Type switch
            {
                FloatRangeType.Ratio => Entry is not IImageSize image || floatRangeLeaf.IsInRange(image.AspectRatio),
                _ => throw new ArgumentOutOfRangeException(nameof(floatRangeLeaf.Type))
            },
            DateLeaf dateLeaf => dateLeaf.Edge switch
            {
                DateRangeEdge.Starting => Entry.CreateDate >= dateLeaf.Date,
                DateRangeEdge.Ending => Entry.CreateDate < dateLeaf.Date,
                _ => throw new ArgumentOutOfRangeException(nameof(dateLeaf.Edge))
            },
            _ => throw new ArgumentOutOfRangeException(nameof(queryToken))
        };

        static bool StringCompare(IQueryToken.Data data, string target)
            => data.IsPrecise ? data.Value == target : target.Contains(data.Value);
    }
}
