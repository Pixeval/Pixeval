// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.UI.Xaml;
using Misaki;
using Pixeval.Database.Managers;
using Pixeval.Utilities;

namespace Pixeval.Pages.Misc;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BrowsingHistoryPage : IScrollViewHost
{
    public BrowsingHistoryPage()
    {
        InitializeComponent();
        SimpleWorkTypeComboBox.SelectedEnum = App.AppViewModel.AppSettings.SimpleWorkType;
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter) => ChangeSource();

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        var type = SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>();
        var isIllustration = type is SimpleWorkType.IllustrationAndManga;
        var source = manager
            .Reverse()
            .SelectNotNull(t => t.Entry)
            .Where(t => t.ImageType is ImageType.Other ^ isIllustration)
            .ToAsyncEnumerable();

        WorkContainer.WorkView.ResetEngine(type switch
        {
            SimpleWorkType.IllustrationAndManga => App.AppViewModel.MakoClient.Computed(source.Select(t=> new ArtworkWrapper(t))),
            _ => App.AppViewModel.MakoClient.Computed(source.OfType<Novel>()),
        });
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;

    private void WorkContainer_OnRefreshRequested(object sender, RoutedEventArgs e) => ChangeSource();
}

public partial class ArtworkWrapper(IArtworkInfo info) : IArtworkInfo
{
    /// <inheritdoc />
    public int Width => info.Width;

    /// <inheritdoc />
    public int Height => info.Height;

    /// <inheritdoc />
    public string Platform => "pixeval";

    /// <inheritdoc />
    public string Id => info.Id;

    /// <inheritdoc />
    public string Title => info.Title;

    /// <inheritdoc />
    public string Description => info.Description;

    /// <inheritdoc />
    public Uri WebsiteUri => info.WebsiteUri;

    /// <inheritdoc />
    public Uri AppUri => info.AppUri;

    /// <inheritdoc />
    public DateTimeOffset CreateDate => info.CreateDate;

    /// <inheritdoc />
    public IPreloadableList<IUser> Authors => info.Authors;

    /// <inheritdoc />
    public IPreloadableList<IUser> Uploaders => info.Uploaders;

    /// <inheritdoc />
    public SafeRating SafeRating => info.SafeRating;

    /// <inheritdoc />
    public ILookup<ITagCategory, ITag> Tags => info.Tags;

    /// <inheritdoc />
    public IReadOnlyCollection<IImageFrame> Thumbnails => info.Thumbnails;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> AdditionalInfo => info.AdditionalInfo;

    /// <inheritdoc />
    public ImageType ImageType => info.ImageType;

    /// <inheritdoc />
    public int TotalFavorite => info.TotalFavorite;

    /// <inheritdoc />
    public int TotalView => info.TotalView;

    /// <inheritdoc />
    public bool IsFavorite => info.IsFavorite;

    /// <inheritdoc />
    public bool IsAiGenerated => info.IsAiGenerated;
}
