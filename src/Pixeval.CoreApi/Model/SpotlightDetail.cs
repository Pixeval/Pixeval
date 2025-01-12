// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Collections.Generic;

namespace Pixeval.CoreApi.Model;

public record SpotlightDetail(Spotlight SpotlightArticle, string Introduction, IEnumerable<Illustration> Illustrations);
