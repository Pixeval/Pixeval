using System.ComponentModel;
using FluentIcons.Common;
using WinUI3Utilities;

namespace Pixeval.Settings;

public abstract class ObservableSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    Symbol headerIcon)
    : SettingsEntryBase<TSettings>(settings, header, description, headerIcon), INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static string SubHeader(WorkTypeEnum workType) => workType switch
    {
        WorkTypeEnum.Illustration => SettingsPageResources.IllustrationOptionEntryHeader,
        WorkTypeEnum.Manga => SettingsPageResources.MangaOptionEntryHeader,
        WorkTypeEnum.Ugoira => SettingsPageResources.UgoiraOptionEntryHeader,
        WorkTypeEnum.Novel => SettingsPageResources.NovelOptionEntryHeader,
        _ => ThrowHelper.ArgumentOutOfRange<WorkTypeEnum, string>(workType)
    };

    public static Symbol SubHeaderIcon(WorkTypeEnum workType) => workType switch
    {
        WorkTypeEnum.Illustration => Symbol.Image,
        WorkTypeEnum.Manga => Symbol.ImageMultiple,
        WorkTypeEnum.Ugoira => Symbol.Gif,
        WorkTypeEnum.Novel => Symbol.BookOpen,
        _ => ThrowHelper.ArgumentOutOfRange<WorkTypeEnum, Symbol>(workType)
    };
}
