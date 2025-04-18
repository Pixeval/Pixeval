// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using FluentIcons.Common;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util.UI;

namespace Pixeval.Controls;


public partial class IllustrationItemViewModel
{
    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveCommand"/>
    public XamlUICommand MangaSaveCommand { get; } = EntryItemResources.MangaSave.GetCommand(Symbol.SaveImage);

    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveAsCommand"/>
    public XamlUICommand MangaSaveAsCommand { get; } = EntryItemResources.MangaSaveAs.GetCommand(Symbol.SaveEdit);
}
