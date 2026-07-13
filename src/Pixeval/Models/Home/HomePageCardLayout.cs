// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Text.Json.Serialization;
using Mako;
using Mako.Global.Enum;
using Pixeval.Models.Options;
using Pixeval.Views.Home;

namespace Pixeval.Models.Home;

public record HomePageCardLayout
{
    public HomePageCardLayout()
    {
    }

    public HomePageCardLayout(HomeCardDefinition definition, int column, int row, int columnSpan = 1, int rowSpan = 1)
    {
        SourceKind = definition.SourceKind;
        if (definition.HasParameter(HomeCardParameterKinds.WorkType))
            WorkType = definition.WorkType;
        if (definition.HasParameter(HomeCardParameterKinds.SimpleWorkType))
            SimpleWorkType = definition.SimpleWorkType;
        if (definition.HasParameter(HomeCardParameterKinds.PrivacyPolicy))
            PrivacyPolicy = definition.PrivacyPolicy;
        if (definition.HasParameter(HomeCardParameterKinds.RankOption))
            RankOption = definition.RankOption;
        Column = column;
        Row = row;
        ColumnSpan = columnSpan;
        RowSpan = rowSpan;
    }

    public HomePageCardSourceKind SourceKind { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public WorkType WorkType { get; set; } = WorkType.Illustration;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public SimpleWorkType SimpleWorkType { get; set; } = SimpleWorkType.Illustration;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PrivacyPolicy PrivacyPolicy { get; set; } = PrivacyPolicy.Public;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public RankOption RankOption { get; set; } = RankOption.Day;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long UserId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long EntryId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long SeriesId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SearchText { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Tag { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public uint BackgroundColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool UseSpecifiedRankingDate { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTimeOffset RankingDate
    {
        get => UseSpecifiedRankingDate ? field : default;
        set;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Column { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Row { get; set; }

    public int ColumnSpan { get; set; } = 1;

    public int RowSpan { get; set; } = 1;

    [JsonIgnore] public HomePageCardTemplateKind TemplateKind => Definition.TemplateKind;

    [JsonIgnore] public HomeCardDefinition Definition => HomeCardDefinitions.Get(SourceKind);

    public string BuildTitle() => Definition.BuildTitle(this);

    public override string ToString() => BuildTitle();

    public DateTimeOffset GetRankingDate() =>
        UseSpecifiedRankingDate && RankingDate != default
            ? RankingDate
            : MakoClient.RankingMaxDateTime;
}
