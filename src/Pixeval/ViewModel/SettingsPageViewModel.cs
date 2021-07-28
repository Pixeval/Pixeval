using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mako.Global.Enum;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public record IllustrationSortOptionWrapper
    {
        public IllustrationSortOption Option { get; }

        public string LocalizedString { get; }

        public IllustrationSortOptionWrapper(IllustrationSortOption option, string localizedString)
        {
            Option = option;
            LocalizedString = localizedString;
        }
    }

    public record SearchTagMatchOptionWrapper
    {
        public SearchTagMatchOption Option { get; }

        public string LocalizedString { get; }

        public SearchTagMatchOptionWrapper(SearchTagMatchOption option, string localizedString)
        {
            Option = option;
            LocalizedString = localizedString;
        }
    }

    public class SettingsPageViewModel : ObservableObject
    {
        private readonly AppSetting _appSetting;

        public SettingsPageViewModel(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public static readonly IEnumerable<ApplicationTheme> AvailableApplicationThemes = typeof(ApplicationTheme).GetEnumValues<ApplicationTheme>();

        /// <summary>
        /// The <see cref="IllustrationSortOption"/> is welded into Mako, we cannot add <see cref="LocalizedResource"/> on its fields
        /// so an alternative way is chosen
        /// </summary>
        public static readonly IEnumerable<IllustrationSortOptionWrapper> AvailableIllustrationSortOptions = new IllustrationSortOptionWrapper[]
        {
            new(IllustrationSortOption.PublishDateDescending, MiscResources.IllustrationSortOptionPublishDateDescending),
            new(IllustrationSortOption.PublishDateAscending, MiscResources.IllustrationSortOptionPublishDateAscending),
            new(IllustrationSortOption.PopularityDescending, MiscResources.IllustrationSortOptionPopularityDescending)
        };

        /// <summary>
        /// The same reason as <see cref="AvailableIllustrationSortOptions"/>
        /// </summary>
        public static readonly IEnumerable<SearchTagMatchOptionWrapper> AvailableSearchTagMatchOption = new SearchTagMatchOptionWrapper[]
        {
            new(SearchTagMatchOption.PartialMatchForTags, MiscResources.SearchTagMatchOptionPartialMatchForTags),
            new(SearchTagMatchOption.ExactMatchForTags, MiscResources.SearchTagMatchOptionExactMatchForTags),
            new(SearchTagMatchOption.TitleAndCaption, MiscResources.SearchTagMatchOptionTitleAndCaption)
        };

        public IllustrationSortOptionWrapper BoxSortOption()
        {
            return AvailableIllustrationSortOptions.First(s => s.Option == DefaultSortOption);
        }

        public void UnboxSortOption(object wrapper)
        {
            DefaultSortOption = ((IllustrationSortOptionWrapper) wrapper).Option;
        }

        public SearchTagMatchOptionWrapper BoxSearchTagMatchOption()
        {
            return AvailableSearchTagMatchOption.First(s => s.Option == TagMatchOption);
        }

        public void UnboxSearchTagMatchOption(object wrapper)
        {
            TagMatchOption = ((SearchTagMatchOptionWrapper) wrapper).Option;
        }

        public ApplicationTheme Theme
        {
            get => _appSetting.Theme;
            set => SetProperty(_appSetting.Theme, value, _appSetting, (setting, value) => setting.Theme = value);
        }

        public ObservableCollection<string> ExcludeTags
        {
            get => _appSetting.ExcludeTags;
            set => SetProperty(_appSetting.ExcludeTags, value, _appSetting, (setting, value) => setting.ExcludeTags = value);
        }

        public bool DisableDomainFronting
        {
            get => _appSetting.DisableDomainFronting;
            set => SetProperty(_appSetting.DisableDomainFronting, value, _appSetting, (setting, value) =>
            {
                setting.DisableDomainFronting = value;
                App.MakoClient.Configuration.Bypass = !value;
            });
        }

        public IllustrationSortOption DefaultSortOption
        {
            get => _appSetting.DefaultSortOption;
            set => SetProperty(_appSetting.DefaultSortOption, value, _appSetting, (setting, value) => setting.DefaultSortOption = value);
        }

        public SearchTagMatchOption TagMatchOption
        {
            get => _appSetting.TagMatchOption;
            set => SetProperty(_appSetting.TagMatchOption, value, _appSetting, (setting, value) => setting.TagMatchOption = value);
        }

        public int PageLimitForKeywordSearch
        {
            get => _appSetting.PageLimitForKeywordSearch;
            set => SetProperty(_appSetting.PageLimitForKeywordSearch, value, _appSetting, (setting, value) => setting.PageLimitForKeywordSearch = value);
        }

        public int SearchStartingFromPageNumber
        {
            get => _appSetting.SearchStartingFromPageNumber;
            set => SetProperty(_appSetting.SearchStartingFromPageNumber, value, _appSetting, (setting, value) => setting.SearchStartingFromPageNumber = value);
        }

        public int PageLimitForSpotlight
        {
            get => _appSetting.PageLimitForSpotlight;
            set => SetProperty(_appSetting.PageLimitForSpotlight, value, _appSetting, (setting, value) => setting.PageLimitForSpotlight = value);
        }

        public string? MirrorHost
        {
            get => _appSetting.MirrorHost;
            set => SetProperty(_appSetting.MirrorHost, value, _appSetting, (setting, value) =>
            {
                setting.MirrorHost = value;
                App.MakoClient.Configuration.MirrorHost = value;
            });
        }

        public int MaxDownloadTaskConcurrencyLevel
        {
            get => _appSetting.MaxDownloadTaskConcurrencyLevel;
            set => SetProperty(_appSetting.MaxDownloadTaskConcurrencyLevel, value, _appSetting, (setting, value) => setting.MaxDownloadTaskConcurrencyLevel = value);
        }

        public void AddR18Filtering()
        {
            ExcludeTags.AddIfNotPresent("R-18", StringComparer.OrdinalIgnoreCase);
            ExcludeTags.AddIfNotPresent("R-18G", StringComparer.OrdinalIgnoreCase);
        }

        public void RemoveR18Filtering()
        {
            ExcludeTags.Remove("R-18");
            ExcludeTags.Remove("R-18G");
        }
    }
}