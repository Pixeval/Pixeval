#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IsUgoiraMacro.cs
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

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IllustrationItemViewModel>]
public class IsUgoiraMacro : IPredicate<IllustrationItemViewModel>
{
    public string Name => "if_gif";

    public bool Match(IllustrationItemViewModel context)
    {
        return context.IsUgoira;
    }
}

/// <summary>
/// 包含R18G
/// </summary>
[MetaPathMacro<IWorkViewModel>]
public class IsR18Macro : IPredicate<IWorkViewModel>
{
    public string Name => "if_r18";

    public bool Match(IWorkViewModel context)
    {
        return context.IsXRestricted;
    }
}

[MetaPathMacro<IWorkViewModel>]
public class IsR18GMacro : IPredicate<IWorkViewModel>
{
    public string Name => "if_r18g";

    public bool Match(IWorkViewModel context)
    {
        return context is { IsXRestricted: true, XRestrictionCaption: BadgeMode.R18G };
    }
}

[MetaPathMacro<IWorkViewModel>]
public class IsAiMacro : IPredicate<IWorkViewModel>
{
    public string Name => "if_ai";

    public bool Match(IWorkViewModel context)
    {
        return context.IsAiGenerated;
    }
}

[MetaPathMacro<IllustrationItemViewModel>]
public class IsIllustrationMacro : IPredicate<IllustrationItemViewModel>
{
    public string Name => "if_illust";

    public bool Match(IllustrationItemViewModel context) => true;
}

[MetaPathMacro<NovelItemViewModel>]
public class IsNovelMacro : IPredicate<NovelItemViewModel>
{
    public string Name => "if_novel";

    public bool Match(NovelItemViewModel context) => true;
}
