// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.ComponentModel;

namespace Pixeval.CoreApi.Global.Enum;

public enum WorkType
{
    [Description("illust")]
    Illust,

    [Description("manga")]
    Manga,

    [Description("novel")]
    Novel
}

public enum SimpleWorkType
{
    IllustAndManga,

    Novel
}
