// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Globalization;
using Mako;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Views.Home;

namespace Pixeval.Models.Home;

public record HomePageCardLayout
{
    public HomePageCardLayout()
    {
    }

    public HomePageCardLayout(HomeCardTemplate template, int column, int row, int columnSpan = 1, int rowSpan = 1)
    {
        SourceKind = template.SourceKind;
        WorkType = template.WorkType;
        SimpleWorkType = template.SimpleWorkType;
        PrivacyPolicy = template.PrivacyPolicy;
        RankOption = template.RankOption;
        RankingDate = MakoClient.RankingMaxDateTime;
        Column = column;
        Row = row;
        ColumnSpan = columnSpan;
        RowSpan = rowSpan;
    }

    public HomePageCardSourceKind SourceKind { get; set; }

    public WorkType WorkType { get; set; } = WorkType.Illustration;

    public SimpleWorkType SimpleWorkType { get; set; } = SimpleWorkType.IllustrationAndManga;

    public PrivacyPolicy PrivacyPolicy { get; set; } = PrivacyPolicy.Public;

    public RankOption RankOption { get; set; } = RankOption.Day;

    public long UserId { get; set; }

    public long EntryId { get; set; }

    public string? SearchText { get; set; }

    public string? Tag { get; set; }

    public uint BackgroundColor { get; set; }

    public bool UseSpecifiedRankingDate { get; set; }

    public DateTimeOffset RankingDate { get; set; }

    public int Column { get; set; }

    public int Row { get; set; }

    public int ColumnSpan { get; set; } = 1;

    public int RowSpan { get; set; } = 1;

    public HomePageCardTemplateKind TemplateKind => SourceKind switch
    {
        HomePageCardSourceKind.WorkRecommended
            or HomePageCardSourceKind.WorkBookmarks
            or HomePageCardSourceKind.WorkRanking
            or HomePageCardSourceKind.WorkNew
            or HomePageCardSourceKind.WorkFollowing
            or HomePageCardSourceKind.WorkPosts
            or HomePageCardSourceKind.WorkSearch
            =>  HomePageCardTemplateKind.WorkList,
        HomePageCardSourceKind.UserRecommended
            or HomePageCardSourceKind.UserSearch
            or HomePageCardSourceKind.UserFollowing
            or HomePageCardSourceKind.UserMyPixiv
            => HomePageCardTemplateKind.UserList,
        HomePageCardSourceKind.Spotlight => HomePageCardTemplateKind.SpotlightList,
        HomePageCardSourceKind.SingleImage => HomePageCardTemplateKind.SingleImage,
        HomePageCardSourceKind.SingleNovel => HomePageCardTemplateKind.SingleNovel,
        HomePageCardSourceKind.SingleUser => HomePageCardTemplateKind.SingleUser,
        _ => throw new ArgumentOutOfRangeException(nameof(SourceKind), SourceKind, null)
    };

    public override string ToString()
    {
        var parts = new List<string> { SymbolComboBoxItem.GetResource(SourceKind) };
        parts.AddRange(GetParameterParts());
        return string.Join(I18NManager.GetResource(HomePageResources.CardTitleParameterSeparator), parts);

        IEnumerable<string> GetParameterParts()
        {
            switch (SourceKind)
            {
                case HomePageCardSourceKind.WorkRecommended or HomePageCardSourceKind.WorkNew:
                    yield return GetDescription(WorkType);
                    break;
                case HomePageCardSourceKind.WorkPosts:
                    yield return $"@{UserId}";
                    yield return GetDescription(WorkType);
                    break;
                case HomePageCardSourceKind.WorkBookmarks:
                    yield return $"@{UserId}";
                    yield return GetDescription(SimpleWorkType);
                    yield return GetDescription(PrivacyPolicy);
                    if (!string.IsNullOrWhiteSpace(Tag))
                        yield return $"#{Tag}";
                    break;
                case HomePageCardSourceKind.WorkRanking:
                    yield return GetDescription(SimpleWorkType);
                    yield return GetRankOptionDescription(this);
                    if (UseSpecifiedRankingDate)
                        yield return GetRankingDate().LocalDateTime.ToString("d", CultureInfo.CurrentCulture);
                    break;
                case HomePageCardSourceKind.WorkFollowing:
                    yield return GetDescription(SimpleWorkType);
                    yield return GetDescription(PrivacyPolicy);
                    break;
                case HomePageCardSourceKind.WorkSearch:
                    yield return GetDescription(SimpleWorkType);
                    yield return SearchText ?? "";
                    break;
                case HomePageCardSourceKind.UserSearch:
                    yield return SearchText ?? "";
                    break;
                case HomePageCardSourceKind.UserFollowing:
                    yield return $"@{UserId}";
                    yield return GetDescription(PrivacyPolicy);
                    break;
                case HomePageCardSourceKind.UserMyPixiv or HomePageCardSourceKind.SingleUser:
                    yield return $"@{UserId}";
                    break;
                case HomePageCardSourceKind.SingleImage or HomePageCardSourceKind.SingleNovel:
                    yield return EntryId.ToString();
                    break;
                default:
                    break;
            }

            yield break;
        }

        static string GetDescription<TEnum>(TEnum value)
            where TEnum : struct, Enum =>
            SymbolComboBoxItem.GetResource(value);

        static string GetRankOptionDescription(HomePageCardLayout card)
        {
            var key = card.SimpleWorkType is SimpleWorkType.Novel ? nameof(Novel) : nameof(Illustration);
            return SymbolComboBoxItem.GetResource(card.RankOption, key);
        }
    }

    public DateTimeOffset GetRankingDate() =>
        UseSpecifiedRankingDate && RankingDate != default
            ? RankingDate
            : MakoClient.RankingMaxDateTime;
}
