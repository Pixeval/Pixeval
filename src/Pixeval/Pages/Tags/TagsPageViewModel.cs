using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls.Illustrate;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.Tags;

public class TagsPageViewModel : ObservableObject, IDisposable
{
    public string WorkingDirectory
    {
        get => App.AppViewModel.AppSetting.TagsManagerWorkingPath;
        set => SetProperty(App.AppViewModel.AppSetting.TagsManagerWorkingPath, value, App.AppViewModel.AppSetting, (setting, value) =>
        {
            setting.TagsManagerWorkingPath = value;
            AppContext.SaveConfig(App.AppViewModel.AppSetting);
            ResetEngine(value);
        });
    }

    public ObservableCollection<string> SelectedTags { get; } = [];

    public TagsPageViewModel() => ResetEngine(WorkingDirectory);

    private void ResetEngine(string value)
    {
        var workingDirectory = new DirectoryInfo(value);
        DataProvider.To<TagsEntryDataProvider>().ResetEngine(workingDirectory.EnumerateFiles("*", SearchOption.AllDirectories));
        SelectedTags.CollectionChanged += (_, _) => DataProvider.View.Filter = SelectedTags.Count is 0
            ? null
            : vm => vm.TagsSet is not null && SelectedTags.Any(selectedTag => vm.TagsSet.Contains(selectedTag));
    }

    public IDataProvider<FileInfo, TagsEntryViewModel> DataProvider { get; } = new TagsEntryDataProvider();

    public void Dispose()
    {
        DataProvider.Dispose();
    }
}
