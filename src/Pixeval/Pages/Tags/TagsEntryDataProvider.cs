using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;

namespace Pixeval.Pages.Tags;

public partial class TagsEntryDataProvider : ObservableObject, IDisposable
{
    public TagsEntryDataProvider() => View.ObserveFilterProperty(nameof(TagsEntryViewModel.Tags));

    public AdvancedObservableCollection<TagsEntryViewModel> View { get; } = new([], true);

    public IncrementalLoadingCollection<TagsEntryIncrementalSource, TagsEntryViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<TagsEntryIncrementalSource, TagsEntryViewModel>)!;
        protected set => View.Source = value;
    }

    public void Dispose()
    {
        if (Source is { } source)
            foreach (var entry in source)
                entry.Dispose();

        View.Clear();
    }

    public void ResetEngine(IEnumerable<FileInfo> source)
    {
        Source = new IncrementalLoadingCollection<TagsEntryIncrementalSource, TagsEntryViewModel>(new TagsEntryIncrementalSource(source));
    }
}
