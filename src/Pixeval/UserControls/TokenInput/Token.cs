#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/Token.cs
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
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Utilities;

namespace Pixeval.UserControls.TokenInput;

public sealed class Token : ObservableObject, IEquatable<Token>, ICloneable
{
    private bool _caseSensitive;

    private bool _isRegularExpression;
    private string _tokenContent;

    public Token(string tokenContent, bool caseSensitive, bool isRegularExpression)
    {
        _tokenContent = tokenContent;
        _caseSensitive = caseSensitive;
        _isRegularExpression = isRegularExpression;
        if (IsRegularExpression && !tokenContent.IsValidRegexPattern())
        {
            throw new ArgumentException(nameof(tokenContent));
        }
    }

    public Token()
    {
        _tokenContent = string.Empty;
    }

    public string TokenContent
    {
        get => _tokenContent;
        set => SetProperty(ref _tokenContent, value);
    }

    public bool CaseSensitive
    {
        get => _caseSensitive;
        set => SetProperty(ref _caseSensitive, value);
    }

    public bool IsRegularExpression
    {
        get => _isRegularExpression;
        set => SetProperty(ref _isRegularExpression, value);
    }

    public object Clone()
    {
        return MemberwiseClone();
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
            return Regex.IsMatch(input!, _tokenContent);
        }

        return CaseSensitive ? input == _tokenContent : input!.Equals(_tokenContent, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CaseSensitive ? TokenContent : TokenContent.ToLower(), CaseSensitive, IsRegularExpression);
    }
}