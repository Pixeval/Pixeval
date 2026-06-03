// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Misaki;
using Pixeval.Filters.Nodes;
using Pixeval.Filters.Values;
using Pixeval.Models.Filters;

namespace Pixeval.ViewModels;

public interface IWorkViewModel : INotifyPropertyChanged
{
    bool IsBookmarkSupported { get; }

    IArtworkInfo Entry { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.AddToBookmarkCommand"/>
    IAsyncRelayCommand<(IReadOnlyList<string>? Tags, bool IsPrivate, Control? Control)> AddToBookmarkCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.BookmarkCommand"/>
    IAsyncRelayCommand<Control?> BookmarkCommand { get; }

    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveCommand"/>
    IAsyncRelayCommand<Control?> SaveCommand { get; }

    bool Filter(FilterNode node) => node switch
    {
        FilterGroupNode sequence => sequence switch
        {
            {
                IsNegated: var isNot,
                Operator: FilterLogicalOperator.And,
                Children: { Count: > 0 } children
            } => isNot ^ children.All(Filter),
            {
                IsNegated: var isNot,
                Operator: FilterLogicalOperator.Or,
                Children: { Count: > 0 } children
            } => isNot ^ children.Any(Filter),
            _ => true
        },
        FilterPredicateNode
        {
            IsNegated: var isNot
        } predicate => isNot ^ FilterPredicate(predicate),
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };

    bool FilterPredicate(FilterPredicateNode predicate)
    {
        return predicate.Syntax.Key switch
        {
            WorkFilterSyntaxKeys.Title => StringCompare((FilterTextValue)predicate.Value!, Entry.Title),
            WorkFilterSyntaxKeys.Author => Entry.Authors.Any(t => StringCompare((FilterTextValue)predicate.Value!, t.Name)),
            WorkFilterSyntaxKeys.Tag => Entry.Tags.Any(t => t.Any(h => StringCompare((FilterTextValue)predicate.Value!, h.Name) || h.TranslatedName is { } translatedName && StringCompare((FilterTextValue)predicate.Value!, translatedName))),
            WorkFilterSyntaxKeys.Bookmark => ((FilterLongRange)predicate.Value!).Contains(Entry.TotalFavorite),
            WorkFilterSyntaxKeys.Ratio => Entry is not IImageSize image || ((FilterDoubleRange)predicate.Value!).Contains(image.AspectRatio),
            WorkFilterSyntaxKeys.StartDate => Entry.CreateDate >= (DateTimeOffset)predicate.Value!,
            WorkFilterSyntaxKeys.EndDate => Entry.CreateDate < (DateTimeOffset)predicate.Value!,
            WorkFilterSyntaxKeys.R18 => (bool)predicate.Value! ^ Entry.SafeRating.IsR18,
            WorkFilterSyntaxKeys.R18G => (bool)predicate.Value! ^ Entry.SafeRating.IsR18G,
            WorkFilterSyntaxKeys.Ai => (bool)predicate.Value! ^ Entry.IsAiGenerated,
            WorkFilterSyntaxKeys.Gif => (bool)predicate.Value! ^ Entry.ImageType is ImageType.SingleAnimatedImage,
            _ => throw new ArgumentOutOfRangeException(nameof(predicate))
        };

        static bool StringCompare(FilterTextValue data, string target)
            => data.IsExact
                ? data.AsSpan().Equals(target, StringComparison.Ordinal)
                : target.Contains(data.AsSpan(), StringComparison.Ordinal);
    }
}
