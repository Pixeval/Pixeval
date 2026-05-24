// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Engine.Implements;
using Mako.Global.Enum;
using Pixeval.I18N;

namespace Pixeval.ViewModels.Search;

public abstract partial class SearchArgumentsFormViewModelBase : ViewModelBase
{
    protected static readonly string CommonUnspecified = I18NManager.GetResource(SearchResources.CommonUnspecified);

    public OptionalDateSearchOptionViewModel StartDateOption { get; } = new();

    public OptionalDateSearchOptionViewModel EndDateOption { get; } = new();

    [ObservableProperty]
    public partial WorkSortOption SortOption { get; set; } = WorkSortOption.PublishDateDescending;

    [ObservableProperty]
    public partial bool AiType { get; set; }

    [ObservableProperty]
    public partial bool MergePlainKeywordResults { get; set; } = true;

    [ObservableProperty]
    public partial bool IncludeTranslatedTagResults { get; set; } = true;

    [ObservableProperty]
    public partial bool IncludePotentialViolationWorks { get; set; }

    public virtual bool TryValidate(out string title, out string content)
    {
        var startDate = StartDateOption.ToNullableDateTimeOffset()?.Date;
        var endDate = EndDateOption.ToNullableDateTimeOffset()?.Date;
        var japanToday = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(9)).Date;

        if (startDate > endDate || startDate > japanToday || endDate > japanToday)
        {
            title = I18NManager.GetResource(SearchResources.ValidationInvalidDateRangeTitle);
            content = I18NManager.GetResource(SearchResources.ValidationInvalidDateRangeContent);
            return false;
        }

        if (!(App.AppViewModel.MakoClient.Me?.IsPremium ?? false)
            && SortOption is WorkSortOption.PopularityDescending)
        {
            title = I18NManager.GetResource(SearchResources.ValidationPremiumSortTitle);
            content = I18NManager.GetResource(SearchResources.ValidationPremiumSortContent);
            return false;
        }

        title = content = "";
        return true;
    }

    protected void ApplyCommonArguments(SearchArgumentsBase arguments)
    {
        arguments.SortOption = SortOption;
        arguments.StartDate = StartDateOption.ToNullableDateTimeOffset();
        arguments.EndDate = EndDateOption.ToNullableDateTimeOffset();
        arguments.AiType = AiType;
        arguments.MergePlainKeywordResults = MergePlainKeywordResults;
        arguments.IncludeTranslatedTagResults = IncludeTranslatedTagResults;
        arguments.IncludePotentialViolationWorks = IncludePotentialViolationWorks;
    }

    protected static bool HasInvalidRange(int? min, int? max) => min is { } minValue && max is { } maxValue && minValue > maxValue;
}
