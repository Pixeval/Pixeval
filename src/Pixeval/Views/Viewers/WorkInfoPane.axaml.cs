using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.Input;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Models.Database.Managers;
using Pixeval.Utilities;
using Pixeval.Views.Capability;

namespace Pixeval.Views.Viewers;

public class WorkInfoPane : TemplatedControl
{
    public static readonly StyledProperty<IArtworkInfo?> ArtworkInfoProperty =
        AvaloniaProperty.Register<WorkInfoPane, IArtworkInfo?>(nameof(ArtworkInfo));

    public static readonly StyledProperty<object?> ActionZoneProperty = AvaloniaProperty.Register<WorkInfoPane, object?>(
        nameof(ActionZone));

    public static readonly StyledProperty<IDataTemplate> ActionZoneTemplateProperty = AvaloniaProperty.Register<WorkInfoPane, IDataTemplate>(
        nameof(ActionZoneTemplate));

    public IDataTemplate ActionZoneTemplate
    {
        get => GetValue(ActionZoneTemplateProperty);
        set => SetValue(ActionZoneTemplateProperty, value);
    }

    public object? ActionZone
    {
        get => GetValue(ActionZoneProperty);
        set => SetValue(ActionZoneProperty, value);
    }

    public IArtworkInfo? ArtworkInfo
    {
        get => GetValue(ArtworkInfoProperty);
        set => SetValue(ArtworkInfoProperty, value);
    }
    
    public IAsyncRelayCommand<IUser?> OpenAuthorCommand { get; }

    public IRelayCommand<ITag?> OpenTagCommand { get; }
    
    public IRelayCommand<ITag?> BlockTagCommand { get; }

    public WorkInfoPane()
    {
        OpenAuthorCommand = new AsyncRelayCommand<IUser?>(OpenAuthorAsync);
        OpenTagCommand = new RelayCommand<ITag?>(OpenTag);
        BlockTagCommand = new RelayCommand<ITag?>(BlockTag);
    }


    private async Task OpenAuthorAsync(IUser? user)
    {
        if (TopLevel.GetTopLevel(this) is not { Launcher: { } launcher, ViewContainer: { } viewContainer })
            return;

        if (user is UserInfo info)
            await viewContainer.CreateUserPageAsync(info.Id);
        else if (user is { })
            await launcher.LaunchUriAsync(user.WebsiteUri);
    }

    private void OpenTag(ITag? tag)
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;
        if (ArtworkInfo is not IWorkEntry entry || tag is null)
            return;

        var type = entry is Illustration ? SimpleWorkType.IllustrationAndManga : SimpleWorkType.Novel;
        SearchHistoryPersistentManager.AddHistory(tag.Name, tag.TranslatedName);
        viewContainer.NavigateTo(new SearchWorksPage(type, tag.Name));
    }
    
    private void BlockTag(ITag? tag)
    {
        if (tag is null) return;

        var blockedTags = App.AppViewModel.AppSettings.BlockedTags;
        if (!blockedTags.Contains(tag.Name))
        {
            blockedTags.Add(tag.Name);
            AppInfo.SaveSettings(App.AppViewModel.AppSettings);
        }
    }
    
    public static IValueConverter HalfVerticalSpaceConverter { get; } = new FuncValueConverter<Rect, double>(x => x.Height / 2);

}
