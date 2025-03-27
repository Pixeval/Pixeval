// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;
using Mako.Model;
using Pixeval.Filters;
using WinUI3Utilities;

namespace Pixeval.Controls;

public interface IWorkViewModel
{
    IWorkEntry Entry { get; }

    long Id { get; }

    int TotalBookmarks { get; }

    bool IsBookmarked { get; set; }

    Tag[] Tags { get; }

    string Title { get; }

    string Caption { get; }

    UserInfo User { get; }

    DateTimeOffset PublishDate { get; }

    bool IsAiGenerated { get; }

    /// <summary>
    /// R18 or R18G
    /// </summary>
    bool IsXRestricted { get; }

    BadgeMode XRestrictionCaption { get; }

    Uri AppUri { get; }

    Uri WebUri { get; }

    Uri PixEzUri { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.AddToBookmarkCommand"/>
    XamlUICommand AddToBookmarkCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.BookmarkCommand"/>
    XamlUICommand BookmarkCommand { get; }

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
                StringType.Title => StringCompare(stringLeaf.Content, Title),
                StringType.Author => StringCompare(stringLeaf.Content, User.Name),
                StringType.Tag => Tags.Any(t => StringCompare(stringLeaf.Content, t.Name)),
                _ => ThrowHelper.ArgumentOutOfRange<StringType, bool>(stringLeaf.Type),
            },
            BoolLeaf boolLeaf => boolLeaf.IsExclude ^ boolLeaf.Type switch
            {
                BoolType.R18 => IsXRestricted,
                BoolType.R18G => XRestrictionCaption is BadgeMode.R18G,
                BoolType.Ai => IsAiGenerated,
                BoolType.Gif => Entry is Illustration { IsUgoira: true },
                _ => ThrowHelper.ArgumentOutOfRange<BoolType, bool>(boolLeaf.Type),
            },
            NumericLeaf numericLeaf => User.Id == numericLeaf.Value,
            NumericRangeLeaf numericRangeLeaf => numericRangeLeaf.Type switch
            {
                NumericRangeType.Bookmark => numericRangeLeaf.IsInRange(TotalBookmarks),
                NumericRangeType.Index => true,
                _ => ThrowHelper.ArgumentOutOfRange<NumericRangeType, bool>(numericRangeLeaf.Type),
            },
            FloatRangeLeaf floatRangeLeaf => floatRangeLeaf.Type switch
            {
                FloatRangeType.Ratio => this is not IllustrationItemViewModel illustration || floatRangeLeaf.IsInRange(illustration.AspectRatio),
                _ => ThrowHelper.ArgumentOutOfRange<FloatRangeType, bool>(floatRangeLeaf.Type),
            },
            DateLeaf dateLeaf => dateLeaf.Edge switch
            {
                DateRangeEdge.Starting => PublishDate >= dateLeaf.Date,
                DateRangeEdge.Ending => PublishDate < dateLeaf.Date,
                _ => ThrowHelper.ArgumentOutOfRange<DateRangeEdge, bool>(dateLeaf.Edge),
            },
            _ => ThrowHelper.ArgumentOutOfRange<QueryLeaf, bool>(queryToken),
        };

        static bool StringCompare(IQueryToken.Data data, string target)
            => data.IsPrecise ? data.Value == target : target.Contains(data.Value);
    }
}
