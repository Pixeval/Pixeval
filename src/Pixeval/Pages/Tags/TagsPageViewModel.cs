using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls.Illustrate;
using WinUI3Utilities;

namespace Pixeval.Pages.Tags;

public class TagsPageViewModel : ObservableObject, IDisposable
{
    public string WorkingDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);

    public ObservableCollection<string> SelectedTags { get; } = [];

    public TagsPageViewModel()
    {
        var workingDirectory = new DirectoryInfo(WorkingDirectory);
        DataProvider.To<TagsEntryDataProvider>().ResetEngine(workingDirectory.EnumerateFiles("*", SearchOption.AllDirectories));
        SelectedTags.CollectionChanged += (_, _) => DataProvider.View.Filter = SelectedTags.Count is 0
            ? null
            : vm => vm.Tags is not null && SelectedTags.Any(selectedTag => vm.Tags.Contains(selectedTag));
    }

    public IDataProvider<FileInfo, TagsEntryViewModel> DataProvider { get; } = new TagsEntryDataProvider();

    public void Dispose()
    {
        DataProvider.Dispose();
    }
}
