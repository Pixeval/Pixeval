// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Engine.Implements;
using Mako.Global.Enum;
using Pixeval.I18N;

namespace Pixeval.ViewModels.Search;

public partial class IllustrationSearchFormViewModel : SearchArgumentsFormViewModelBase
{
    public IllustrationSearchFormViewModel()
    {
        MatchOption = App.AppViewModel.AppSettings.SearchIllustrationTagMatchOption;
    }

    [ObservableProperty]
    public partial SearchIllustrationTagMatchOption MatchOption { get; set; }

    [ObservableProperty]
    public partial SearchIllustrationContentType ContentType { get; set; } = SearchIllustrationContentType.IllustrationAndMangaAndUgoira;

    [ObservableProperty]
    public partial SearchIllustrationRatioPattern RatioPattern { get; set; } = SearchIllustrationRatioPattern.All;

    [ObservableProperty]
    public partial IReadOnlyList<string> ToolItems { get; set; } = [CommonUnspecified];

    [ObservableProperty]
    public partial string Tool { get; set; } = CommonUnspecified;

    public OptionalIntSearchOptionViewModel WidthMinOption { get; } = new();

    public OptionalIntSearchOptionViewModel WidthMaxOption { get; } = new();

    public OptionalIntSearchOptionViewModel HeightMinOption { get; } = new();

    public OptionalIntSearchOptionViewModel HeightMaxOption { get; } = new();

    public override bool TryValidate(out string title, out string content)
    {
        if (!base.TryValidate(out title, out content))
            return false;

        if (HasInvalidRange(WidthMinOption.NullableInt, WidthMaxOption.NullableInt)
            || HasInvalidRange(HeightMinOption.NullableInt, HeightMaxOption.NullableInt))
        {
            title = I18NManager.GetResource(SearchResources.ValidationInvalidNumericRangeTitle);
            content = I18NManager.GetResource(SearchResources.ValidationInvalidNumericRangeContent);
            return false;
        }

        title = content = "";
        return true;
    }

    public IllustrationSearchArguments BuildArguments(string searchText)
    {
        var arguments = new IllustrationSearchArguments(searchText)
        {
            MatchOption = MatchOption,
            ContentType = ContentType,
            RatioPattern = RatioPattern,
            WidthMin = WidthMinOption.NullableInt,
            WidthMax = WidthMaxOption.NullableInt,
            HeightMin = HeightMinOption.NullableInt,
            HeightMax = HeightMaxOption.NullableInt,
            Tool = Tool == CommonUnspecified ? null : Tool
        };

        ApplyCommonArguments(arguments);
        return arguments;
    }
}
