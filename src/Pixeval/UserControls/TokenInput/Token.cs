using System;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Utilities;

namespace Pixeval.UserControls.TokenInput
{
    public sealed class Token : ObservableObject, IEquatable<Token>, ICloneable
    {
        private string _tokenContent;

        public string TokenContent
        {
            get => _tokenContent;
            set => SetProperty(ref _tokenContent, value);
        }

        private bool _caseSensitive;

        public bool CaseSensitive
        {
            get => _caseSensitive;
            set => SetProperty(ref _caseSensitive, value);
        }

        private bool _isRegularExpression;

        public bool IsRegularExpression
        {
            get => _isRegularExpression;
            set => SetProperty(ref _isRegularExpression, value);
        }

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

        public void Deconstruct(out string tokenContent, out bool caseSensitive, out bool isRegularExpression)
        {
            tokenContent = TokenContent;
            caseSensitive = CaseSensitive;
            isRegularExpression = IsRegularExpression;
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

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}