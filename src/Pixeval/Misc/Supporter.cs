#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/Supporter.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Pixeval.Misc;

public record Supporter(string Nickname, string Name, ImageSource ProfilePicture, Uri ProfileUri)
{
    // ReSharper disable StringLiteralTypo
    private static readonly IEnumerable<(int Id, string Nickname, string Name)> _supporters =
    [
        (22517862, "Sep", "Guro2"),
        (46415928, "无论时间", "wulunshijian"),
        (12800094, "CN", "ControlNet"),
        (14993992, "CY", "Cyl18"),
        (40987061, "对味", "duiweiya"),
        (6669365, "LG", "LasmGratel"),
        (76583116, "鱼鱼", "sovetskyfish"),
        (96558937, "探姬", "PerolNotsfsssf"),
        (29229273, "Summpot", "Summpot"),
        (62325494, "扑克", "Poker-sang"),
        (49679244, "南门二", "Rigil-Kentaurus"),
        (35005476, "当妈", "TheRealKamisama"),
        (5109850, "茶栗", "cqjjjzr"),
        (27049838, "cnbluefire", "cnbluefire")
    ];
    // ReSharper restore StringLiteralTypo

    public static readonly IEnumerable<Supporter> Supporters = _supporters.Select(t =>
        new Supporter(t.Nickname, '@' + t.Name, SupportImageOf(t.Id), new Uri("https://github.com/" + t.Name)));

    private static ImageSource SupportImageOf(int supporterId)
    {
        return new BitmapImage(new Uri($"https://avatars.githubusercontent.com/u/{supporterId}?v=4"));
    }
}
