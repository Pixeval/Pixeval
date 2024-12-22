#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/Token.cs
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
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class Token : ObservableObject, IEquatable<Token>, IDeepCloneable<Token>
{
    public static readonly Token Empty = new("", false, false);

    [ObservableProperty]
    public partial bool CaseSensitive { get; set; }

    [ObservableProperty]
    public partial bool IsRegularExpression { get; set; }

    [ObservableProperty]
    public partial string TokenContent { get; set; }

    [ObservableProperty]
    public partial string Tooltip { get; set; }

    public Token(string tokenContent, bool caseSensitive, bool isRegularExpression, string tooltip = "")
    {
        TokenContent = tokenContent;
        CaseSensitive = caseSensitive;
        IsRegularExpression = isRegularExpression;
        Tooltip = tooltip;
        if (IsRegularExpression && !tokenContent.IsValidRegexPattern())
            ThrowHelper.Argument(tokenContent);
    }

    public Token()
    {
        TokenContent = "";
        Tooltip = "";
    }

    /// <summary>
    /// 成员全部是类似于值类型，所以深拷贝和浅拷贝效果一样
    /// </summary>
    /// <returns></returns>
    public Token DeepClone()
    {
        return (Token)MemberwiseClone();
    }

    public bool Equals(Token? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return CaseSensitive
            ? TokenContent == other.TokenContent
            : TokenContent.Equals(other.TokenContent, StringComparison.OrdinalIgnoreCase);
    }

    public void Deconstruct(out string tokenContent, out bool caseSensitive, out bool isRegularExpression)
    {
        tokenContent = TokenContent;
        caseSensitive = CaseSensitive;
        isRegularExpression = IsRegularExpression;
    }

    public bool Match(string? input)
    {
        if (TokenContent.IsNullOrEmpty())
        {
            return true;
        }

        if (input.IsNullOrEmpty())
        {
            return false;
        }

        if (IsRegularExpression)
        {
            return Regex.IsMatch(input!, TokenContent);
        }

        return CaseSensitive ? input == TokenContent : input!.Equals(TokenContent, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CaseSensitive ? TokenContent : TokenContent.ToLower(), CaseSensitive, IsRegularExpression);
    }
}
