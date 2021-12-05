using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Util.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Pages
{
    public class SuggestionModel : IEquatable<SuggestionModel?>
    {
        public FontIcon? Icon
        {
            get
            {
                return SuggestionType switch
                {
                    SuggestionType.Tag => FontIconSymbols.TagE8EC.GetFontIcon(12),
                    SuggestionType.Settings => FontIconSymbols.SettingE713.GetFontIcon(12),
                    SuggestionType.History => FontIconSymbols.HistoryE81C.GetFontIcon(12),
                    _ => null,
                };
            }
        }

        public Visibility TranslatedNameVisibility
        {
            get => TranslatedName == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public SuggestionType SuggestionType { get; init; }

        public string? Name { get; init; }

        public string? TranslatedName { get; init; }

        public static SuggestionModel FromTag(Tag tag)
        {
            return new SuggestionModel
            {
                Name = tag.Name,
                TranslatedName = tag.TranslatedName,
                SuggestionType = SuggestionType.Tag
            };
        }

        public static SuggestionModel FromHistory(SearchHistoryEntry history)
        {
            return new SuggestionModel
            {
                Name = history.Value,
                SuggestionType = SuggestionType.History
            };
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SuggestionModel);
        }

        public bool Equals(SuggestionModel? other)
        {
            return other != null &&
                   SuggestionType == other.SuggestionType &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SuggestionType, Name);
        }
    }

    public enum SuggestionType
    {
        Tag, Settings, History
    }
}
