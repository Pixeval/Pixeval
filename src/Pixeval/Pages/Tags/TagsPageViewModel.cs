using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls.Illustrate;
using WinUI3Utilities;

namespace Pixeval.Pages.Tags;

public class TagsPageViewModel : ObservableObject, IDisposable
{
    public string WorkingDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);

    public TagsPageViewModel()
    {
        var workingDirectory = new DirectoryInfo(WorkingDirectory);
        DataProvider.To<TagsEntryDataProvider>().ResetEngine(workingDirectory.EnumerateFiles("*", SearchOption.AllDirectories));
    }

    public IDataProvider<FileInfo, TagsEntryViewModel> DataProvider { get; } = new TagsEntryDataProvider();

    public void Dispose()
    {
        DataProvider.Dispose();
    }
}
