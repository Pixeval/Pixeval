using System.ComponentModel;
using WinUI3Utilities;
using WinUI3Utilities.Controls;

namespace Pixeval.Settings;

public abstract class ObservableSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    IconGlyph headerIcon)
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

    public static IconGlyph SubHeaderIcon(WorkTypeEnum workType) => workType switch
    {
        WorkTypeEnum.Illustration => IconGlyph.PhotoE91B,
        WorkTypeEnum.Manga => IconGlyph.PictureE8B9,
        WorkTypeEnum.Ugoira => IconGlyph.GIFF4A9,
        WorkTypeEnum.Novel => IconGlyph.ReadingModeE736,
        _ => ThrowHelper.ArgumentOutOfRange<WorkTypeEnum, IconGlyph>(workType)
    };
}
