// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

namespace Pixeval.CoreApi.Global;

/// <summary>
/// Indicates that the each of its implementation contains a <see cref="MakoClient" />
/// that is to be used as a context provider, whereby "context provider" mostly refers to
/// the properties that are required when performing some context-aware tasks, such as the
/// access token while sending a request to app-api.pixiv.net
/// </summary>
public interface IMakoClientSupport
{
    /// <summary>
    /// The <see cref="MakoClient" /> that tends to be used as a context provider
    /// </summary>
    MakoClient MakoClient { get; }
}