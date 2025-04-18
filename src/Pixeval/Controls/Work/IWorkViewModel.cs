// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;
using Pixeval.Filters;
using WinUI3Utilities;
using Misaki;

namespace Pixeval.Controls;

public interface IWorkViewModel
{
    IArtworkInfo Entry { get; }

    BadgeMode SafeBadgeMode { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.AddToBookmarkCommand"/>
    XamlUICommand? AddToBookmarkCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.BookmarkCommand"/>
    XamlUICommand? BookmarkCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveCommand"/>
    XamlUICommand SaveCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.TryLoadThumbnailAsync"/>
    ValueTask<bool> TryLoadThumbnailAsync(object key);

    /// <inheritdoc cref="WorkEntryViewModel{T}.UnloadThumbnail"/>
    void UnloadThumbnail(object key);

    public bool Filter(TreeNodeBase node) => node switch
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
        _ => ThrowHelper.ArgumentOutOfRange<TreeNodeBase, bool>(node),
    };

    public bool FilterQuery(QueryLeaf queryToken)
    {
        return queryToken switch
        {
            StringLeaf stringLeaf => stringLeaf.Type switch
            {
                StringType.Title => StringCompare(stringLeaf.Content, Entry.Title),
                StringType.Author => Entry.Authors.Any(t => StringCompare(stringLeaf.Content, t.Name)),
                StringType.Tag => Entry.Tags.Any(t => t.Any(h => StringCompare(stringLeaf.Content, h.Name) || StringCompare(stringLeaf.Content, h.TranslatedName))),
                _ => ThrowHelper.ArgumentOutOfRange<StringType, bool>(stringLeaf.Type),
            },
            BoolLeaf boolLeaf => boolLeaf.IsExclude ^ boolLeaf.Type switch
            {
                BoolType.R18 => Entry.SafeRating.IsR18,
                BoolType.R18G => Entry.SafeRating.IsR18G,
                BoolType.Ai => Entry.IsAiGenerated,
                BoolType.Gif => Entry.ImageType is ImageType.SingleAnimatedImage,
                _ => ThrowHelper.ArgumentOutOfRange<BoolType, bool>(boolLeaf.Type),
            },
            // TODO NumericLeaf numericLeaf => Authors.Any(t => (numericLeaf.Value, t.Name)),
            NumericRangeLeaf numericRangeLeaf => numericRangeLeaf.Type switch
            {
                NumericRangeType.Bookmark => numericRangeLeaf.IsInRange(Entry.TotalFavorite),
                NumericRangeType.Index => true,
                _ => ThrowHelper.ArgumentOutOfRange<NumericRangeType, bool>(numericRangeLeaf.Type),
            },
            FloatRangeLeaf floatRangeLeaf => floatRangeLeaf.Type switch
            {
                FloatRangeType.Ratio => Entry is not IImageSize image || floatRangeLeaf.IsInRange(image.AspectRatio),
                _ => ThrowHelper.ArgumentOutOfRange<FloatRangeType, bool>(floatRangeLeaf.Type),
            },
            DateLeaf dateLeaf => dateLeaf.Edge switch
            {
                DateRangeEdge.Starting => Entry.CreateDate >= dateLeaf.Date,
                DateRangeEdge.Ending => Entry.CreateDate < dateLeaf.Date,
                _ => ThrowHelper.ArgumentOutOfRange<DateRangeEdge, bool>(dateLeaf.Edge),
            },
            _ => ThrowHelper.ArgumentOutOfRange<QueryLeaf, bool>(queryToken),
        };

        static bool StringCompare(IQueryToken.Data data, string target)
            => data.IsPrecise ? data.Value == target : target.Contains(data.Value);
    }
}
