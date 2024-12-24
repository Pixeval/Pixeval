#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/NavigationViewTag.cs
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
using FluentIcons.Common;
using FluentIcons.WinUI;
using Pixeval.Utilities;

namespace Pixeval.Controls.Windowing;

public record NavigationViewTag(Type NavigateTo, object? Parameter, int? Index = null)
{
    public object? Parameter { get; set; } = Parameter;

    public string? Content { get; init; }

    public Symbol? Symbol { get; init; }

    public SymbolIcon? SymbolIcon => Symbol?.Let(t => new SymbolIcon { Symbol = t });
}

public sealed record NavigationViewTag<TPage>(int? Index = null) : NavigationViewTag(typeof(TPage), null, Index);

public sealed record NavigationViewTag<TPage, TParam>(TParam Parameter, int? Index = null) : NavigationViewTag(typeof(TPage), Parameter, Index)
{
    public new TParam Parameter
    {
        get => (TParam)base.Parameter!;
        set => base.Parameter = value;
    }
}
