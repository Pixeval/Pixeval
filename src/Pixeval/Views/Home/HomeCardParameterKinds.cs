// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;

namespace Pixeval.Views.Home;

[Flags]
public enum HomeCardParameterKinds
{
    None = 0,
    WorkType = 1 << 0,
    SimpleWorkType = 1 << 1,
    PrivacyPolicy = 1 << 2,
    RankOption = 1 << 3,
    RankingDate = 1 << 4,
    UserId = 1 << 5,
    EntryId = 1 << 6,
    SearchText = 1 << 7,
    Tag = 1 << 8
}
