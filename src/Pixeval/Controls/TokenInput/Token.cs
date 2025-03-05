// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
        return (Token) MemberwiseClone();
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
