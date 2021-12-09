#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/Supporter.cs
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
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Pixeval.Pages.Misc;

public record Supporter(string Nickname, string Name, ImageSource? ProfilePicture, Uri ProfileUri)
{
    private static ImageSource SupportImageOf(string supporterImgFileName)
    {
        return new BitmapImage(new Uri($"ms-appx:///Assets/Images/Supporters/{supporterImgFileName}"));
    }

    // ReSharper disable StringLiteralTypo
    public static readonly IEnumerable<Supporter> Supporters = new List<Supporter>
    {
        new("Sep", "@Guro2", SupportImageOf("sep.jpg"), new Uri("https://github.com/Guro2")),
        new("无论时间", "@wulunshijian", SupportImageOf("wulunshijian.jpg"), new Uri("https://github.com/wulunshijian")),
        new("CN", "@ControlNet", SupportImageOf("controlnet.png"), new Uri("https://github.com/ControlNet")),
        new("CY", "@Cyl18", SupportImageOf("cyl18.png"), new Uri("https://github.com/cyl18")),
        new("对味", "@duiweiya", SupportImageOf("duiweiya.jpg"), new Uri("https://github.com/duiweiya")),
        new("LG", "@Lasm_Gratel", SupportImageOf("lasm_gratel.jpg"), new Uri("https://github.com/LasmGratel")),
        new("鱼鱼", "@sovetskyfish", null, new Uri("https://github.com/sovetskyfish")),
        new("探姬", "@Perol_Notsfsssf", SupportImageOf("perol_notsfsssf.jpg"), new Uri("https://github.com/Notsfsssf")),
        new("Summpot", "@Summpot", SupportImageOf("summpot.jpg"), new Uri("https://github.com/Summpot")),
        new("扑克", "@Poker-sang", SupportImageOf("poker_sang.jpg"), new Uri("https://github.com/Poker-sang")),
        new("南门二", "@Rigil-Kentaurus", SupportImageOf("rigil_kentaurus.png"), new Uri("https://github.com/Rigil-Kentaurus")),
        new("当妈", "@TheRealKamisama", SupportImageOf("therealkamisama.png"), new Uri("https://github.com/TheRealKamisama")),
        new("茶栗", "@CharlieJiang", SupportImageOf("charlie_jiang.jpg"), new Uri("https://github.com/cqjjjzr"))
    };
    // ReSharper restore StringLiteralTypo
}