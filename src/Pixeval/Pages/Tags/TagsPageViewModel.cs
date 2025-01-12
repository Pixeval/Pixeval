// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.AppManagement;
using WinUI3Utilities;

namespace Pixeval.Pages.Tags;

public partial class TagsPageViewModel : ObservableObject, IDisposable
{
    public string WorkingDirectory
    {
        get => App.AppViewModel.AppSettings.TagsManagerWorkingPath;
        set => SetProperty(App.AppViewModel.AppSettings.TagsManagerWorkingPath, value, App.AppViewModel.AppSettings, (setting, value) =>
        {
            setting.TagsManagerWorkingPath = value;
            AppInfo.SaveConfig(App.AppViewModel.AppSettings);
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

    public TagsEntryDataProvider DataProvider { get; } = new TagsEntryDataProvider();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DataProvider.Dispose();
    }
}
