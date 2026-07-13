// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Home;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels.Home;

namespace Pixeval.Views.Home;

public sealed class HomeCardDefinition(
    HomePageCardSourceKind sourceKind,
    HomePageCardTemplateKind templateKind,
    HomeCardParameterKinds parameters,
    Func<HomePageCardLayout, Task<HomeCardPreviewSource>> previewSourceFactory,
    Action<HomePageCardLayout, HomeCardPreviewSource, TopLevel> pageOpener,
    Func<HomePageCardLayout, IReadOnlyList<string>>? titleParameterFactory = null,
    int defaultColumnSpan = 2,
    int defaultRowSpan = 2,
    WorkType workType = WorkType.Illustration,
    SimpleWorkType simpleWorkType = SimpleWorkType.Illustration,
    PrivacyPolicy privacyPolicy = PrivacyPolicy.Public,
    RankOption rankOption = RankOption.Day,
    bool useCurrentUserAsDefault = false)
{
    public HomePageCardSourceKind SourceKind { get; } = sourceKind;

    public HomePageCardTemplateKind TemplateKind { get; } = templateKind;

    public Symbol Symbol => AvaloniaHelper.GetHomeCardHeader(SourceKind).Symbol;

    public HomeCardParameterKinds Parameters { get; } = parameters;

    public int DefaultColumnSpan { get; } = defaultColumnSpan;

    public int DefaultRowSpan { get; } = defaultRowSpan;

    public WorkType WorkType { get; } = workType;

    public SimpleWorkType SimpleWorkType { get; } = simpleWorkType;

    public PrivacyPolicy PrivacyPolicy { get; } = privacyPolicy;

    public RankOption RankOption { get; } = rankOption;

    public bool UseCurrentUserAsDefault { get; } = useCurrentUserAsDefault;

    public string Title => AvaloniaHelper.GetHomeCardHeader(SourceKind).Header;

    public string Description => SymbolComboBoxItem.GetResource(SourceKind, nameof(Description));

    public bool HasParameter(HomeCardParameterKinds parameter) =>
        (Parameters & parameter) is not HomeCardParameterKinds.None;

    public Task<HomeCardPreviewSource> CreatePreviewSourceAsync(HomePageCardLayout card) =>
        previewSourceFactory(card);

    public void OpenCardPage(HomePageCardLayout card, HomeCardPreviewSource source, TopLevel topLevel)
    {
        try
        {
            pageOpener(card, source, topLevel);
        }
        finally
        {
            source.Dispose();
        }
    }

    public string BuildTitle(HomePageCardLayout card)
    {
        var parts = new List<string> { Title };
        if (titleParameterFactory is not null)
            parts.AddRange(titleParameterFactory(card));

        return string.Join(I18NManager.GetResource(HomePageResources.CardTitleParameterSeparator), parts);
    }
}
