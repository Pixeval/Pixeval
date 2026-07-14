// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using Mako.Global.Enum;
using Pixeval.Models.Home;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement;

public record HomePageCardsSettings
{
    public ObservableCollection<HomePageCardLayout> Cards { get; set; } = CreateDefaultCards();

    public static ObservableCollection<HomePageCardLayout> CreateDefaultCards() =>
    [
        new(HomePageCardSourceKind.Spotlight, 0, 0, 1, 2),
        new(HomePageCardSourceKind.UserRecommended, 0, 2, 1, 2),
        new(HomePageCardSourceKind.WorkRecommended, 0, 4, 1, 3) { SimpleWorkType = SimpleWorkType.Illustration }
    ];
}
