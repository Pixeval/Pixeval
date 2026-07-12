// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Engine.Implements;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.I18N;

namespace Pixeval.ViewModels.Search;

public partial class NovelSearchFormViewModel : SearchArgumentsFormViewModelBase
{
    [ObservableProperty]
    public partial SearchNovelTagMatchOption MatchOption { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<SearchOptionsLanguage> LanguageItems { get; set; } =
    [
        new() { Code = "", Name = CommonUnspecified }
    ];

    [ObservableProperty]
    public partial string Language { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContentLengthEnabled))]
    public partial SearchNovelContentLengthOption ContentLengthOption { get; set; }

    [ObservableProperty]
    public partial bool IsOriginalOnly { get; set; }

    [ObservableProperty]
    public partial bool IsReplaceableOnly { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<SearchOptionsGenre> GenreItems { get; set; } =
    [
        new() { Id = 0, Label = CommonUnspecified }
    ];

    [ObservableProperty]
    public partial int GenreId { get; set; } = 0;

    public OptionalIntSearchOptionViewModel ContentLengthMinOption { get; } = new();

    public OptionalIntSearchOptionViewModel ContentLengthMaxOption { get; } = new();

    public bool IsContentLengthEnabled => ContentLengthOption is not SearchNovelContentLengthOption.None;

    public override bool TryValidate(out string title, out string content)
    {
        if (!base.TryValidate(out title, out content))
            return false;

        if (IsContentLengthEnabled && HasInvalidRange(ContentLengthMinOption.NullableInt, ContentLengthMaxOption.NullableInt))
        {
            title = I18NManager.GetResource(SearchResources.ValidationInvalidNumericRangeTitle);
            content = I18NManager.GetResource(SearchResources.ValidationInvalidNumericRangeContent);
            return false;
        }

        title = content = "";
        return true;
    }

    public NovelSearchArguments BuildArguments(string searchText)
    {
        var hasContentLength = IsContentLengthEnabled;
        var arguments = new NovelSearchArguments(searchText)
        {
            MatchOption = MatchOption,
            LangCode = Language is "" ? null : Language,
            Option = ContentLengthOption,
            ContentLengthMin = hasContentLength ? ContentLengthMinOption.NullableInt : null,
            ContentLengthMax = hasContentLength ? ContentLengthMaxOption.NullableInt : null,
            IsOriginalOnly = IsOriginalOnly,
            GenreId = IsOriginalOnly ? GenreId is 0 ? null : GenreId : null,
            IsReplaceableOnly = IsReplaceableOnly
        };

        ApplyCommonArguments(arguments);
        return arguments;
    }
}
