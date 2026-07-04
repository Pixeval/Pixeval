// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Mako.Model;
using Pixeval.Controls;

namespace Pixeval.ViewModels;

public class SpotlightItemViewModel(Spotlight spotlight) : ThumbnailEntryViewModel<Spotlight>(spotlight),
    IFactory<Spotlight, SpotlightItemViewModel>
{
    public static SpotlightItemViewModel CreateInstance(Spotlight entry) => new(entry);

    public override string ThumbnailUrl => Entry.Thumbnail;

    public override Uri AppUri => Entry.AppUri;

    public override Uri WebsiteUri => Entry.WebsiteUri;
}
