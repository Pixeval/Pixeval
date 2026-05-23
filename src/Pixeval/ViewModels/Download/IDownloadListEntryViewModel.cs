// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using Pixeval.Download;

namespace Pixeval.ViewModels;

public interface IDownloadListEntryViewModel
{
    IReadOnlyList<DownloadItemViewModel> DownloadItems { get; }

    DownloadState CurrentState { get; }

    bool MatchesSearch(string key);

    bool MatchesOption(DownloadListOption option, ISet<IDownloadListEntryViewModel>? customSearchResult);
}
