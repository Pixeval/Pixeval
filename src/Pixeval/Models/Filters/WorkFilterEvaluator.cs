// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Misaki;
using Pixeval.Filters.Nodes;
using Pixeval.Filters.Values;

namespace Pixeval.Models.Filters;

public static class WorkFilterEvaluator
{
    public static bool Filter(IArtworkInfo entry, FilterNode node) => node switch
    {
        FilterGroupNode sequence => sequence switch
        {
            {
                IsNegated: var isNot,
                Operator: FilterLogicalOperator.And,
                Children: { Count: > 0 } children
            } => isNot ^ children.All(child => Filter(entry, child)),
            {
                IsNegated: var isNot,
                Operator: FilterLogicalOperator.Or,
                Children: { Count: > 0 } children
            } => isNot ^ children.Any(child => Filter(entry, child)),
            _ => true
        },
        FilterPredicateNode
        {
            IsNegated: var isNot
        } predicate => isNot ^ FilterPredicate(entry, predicate),
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };

    private static bool FilterPredicate(IArtworkInfo entry, FilterPredicateNode predicate) =>
        predicate.Syntax.Key switch
        {
            WorkFilterSyntaxKeys.Title => StringCompare((FilterTextValue) predicate.Value!, entry.Title),
            WorkFilterSyntaxKeys.Author => entry.Authors.Any(t =>
                StringCompare((FilterTextValue) predicate.Value!, t.Name)),
            WorkFilterSyntaxKeys.Tag => entry.Tags.Any(t => t.Any(h =>
                StringCompare((FilterTextValue) predicate.Value!, h.Name)
                || h.TranslatedName is { } translatedName
                && StringCompare((FilterTextValue) predicate.Value!, translatedName))),
            WorkFilterSyntaxKeys.Bookmark => ((FilterLongRange) predicate.Value!).Contains(entry.TotalFavorite),
            WorkFilterSyntaxKeys.Ratio => entry is not IImageSize image
                                          || ((FilterDoubleRange) predicate.Value!).Contains(image.AspectRatio),
            WorkFilterSyntaxKeys.StartDate => entry.CreateDate >= (DateTimeOffset) predicate.Value!,
            WorkFilterSyntaxKeys.EndDate => entry.CreateDate < (DateTimeOffset) predicate.Value!,
            WorkFilterSyntaxKeys.R18 => (bool) predicate.Value! ^ entry.SafeRating.IsR18,
            WorkFilterSyntaxKeys.R18G => (bool) predicate.Value! ^ entry.SafeRating.IsR18G,
            WorkFilterSyntaxKeys.Ai => (bool) predicate.Value! ^ entry.IsAiGenerated,
            WorkFilterSyntaxKeys.Gif => (bool) predicate.Value! ^ (entry.ImageType is ImageType.SingleAnimatedImage),
            _ => throw new ArgumentOutOfRangeException(nameof(predicate))
        };

    private static bool StringCompare(FilterTextValue data, string target) =>
        data.IsExact
            ? data.AsSpan().Equals(target, StringComparison.Ordinal)
            : target.Contains(data.AsSpan(), StringComparison.Ordinal);
}
