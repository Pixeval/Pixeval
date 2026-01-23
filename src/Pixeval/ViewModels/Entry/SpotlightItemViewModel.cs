// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public class SpotlightItemViewModel(Spotlight spotlight) : ThumbnailEntryViewModel<Spotlight>(spotlight),
    IFactory<Spotlight, SpotlightItemViewModel>
{
    public static SpotlightItemViewModel CreateInstance(Spotlight entry) => new(entry);

    protected override string ThumbnailUrl => Entry.Thumbnail;

    public override Uri AppUri => MakoHelper.GenerateSpotlightAppUri(Entry.Id);

    public override Uri WebsiteUri => MakoHelper.GenerateSpotlightWebUri(Entry.Id);
}
