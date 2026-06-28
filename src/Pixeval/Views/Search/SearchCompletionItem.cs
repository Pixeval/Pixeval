// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Views.Search;

public sealed record SearchCompletionItem(SearchCompletionKind Kind, string Text, string? Description = null);
