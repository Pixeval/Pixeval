// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Misaki;
using Pixeval.Models.Database;

namespace Pixeval.Models.Download;

public sealed record ParserContext(
    IArtworkInfo ArtworkInfo,
    WorkSubscriptionEntry? WorkSubscription = null);
