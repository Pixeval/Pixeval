// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pixeval.Filters;

/// <summary>
/// 负责把外部注册的过滤语法组织成可解析、可补全的语言对象。
/// </summary>
public sealed class FilterLanguage
{
    private static readonly IReadOnlyCollection<FilterCompletionDefinition> _DefaultIntrinsicCompletions =
    [
        new("builtin.and", "and", "and"),
        new("builtin.or", "or", "or"),
        new("builtin.not", "!", "!")
    ];

    private readonly IReadOnlyCollection<FilterSyntaxMatch> _matches;
    private readonly Dictionary<char, IReadOnlyCollection<FilterSyntaxMatch>> _matchesByFirstChar;
    private readonly IReadOnlyCollection<FilterCompletionDefinition> _intrinsicCompletions;
    private readonly HashSet<char> _specialStarters;
    private readonly FilterSyntaxMatch? _defaultTextMatch;

    /// <summary>
    /// 根据外部语法定义构建过滤语言。
    /// </summary>
    public FilterLanguage(IReadOnlyCollection<FilterSyntax> syntaxes, IReadOnlyCollection<FilterCompletionDefinition>? intrinsicCompletions = null)
    {
        var expanded = syntaxes.SelectMany(syntax => syntax.Patterns.SelectMany(pattern => pattern.Expand(syntax))).ToArray();
        _intrinsicCompletions = intrinsicCompletions?.ToArray() ?? _DefaultIntrinsicCompletions;
        _defaultTextMatch = expanded.FirstOrDefault(match => match.Syntax.ValueKind is FilterValueKind.Text && match.HeaderText.Length is 0);
        _matches = [.. expanded.Where(match => match.HeaderText.Length > 0)];
        _matchesByFirstChar = _matches.GroupBy(match => char.ToUpperInvariant(match.HeaderText[0]))
            .ToDictionary(group => group.Key, IReadOnlyCollection<FilterSyntaxMatch> (group) => [.. group]);
        _specialStarters = [.. _matches.Select(match => match.HeaderText[0]).Where(ch => !char.IsLetterOrDigit(ch))];
    }

    /// <summary>
    /// 解析输入文本，并同时产出诊断与补全建议。
    /// </summary>
    public FilterAnalysisResult Analyze(string? text, int caretPosition = -1, FilterValueCompletionProvider? valueCompletionProvider = null)
    {
        var normalized = text ?? "";
        var parser = new Parser(this, normalized);
        var query = parser.Parse();
        var caret = Math.Clamp(caretPosition < 0 ? normalized.Length : caretPosition, 0, normalized.Length);
        return new(query, parser.Diagnostics, GetCompletions(normalized, caret, valueCompletionProvider));
    }

    /// <summary>
    /// 根据当前输入和光标位置生成补全候选。
    /// </summary>
    private IReadOnlyList<FilterCompletionItem> GetCompletions(string text, int caret, FilterValueCompletionProvider? valueCompletionProvider)
    {
        var tokenStart = FindTokenStart(text, caret);
        var tokenEnd = FindTokenEnd(text, caret);
        var replacementSpan = FilterTextSpan.FromBounds(tokenStart, tokenEnd);
        var fragment = text.AsSpan(tokenStart, Math.Max(caret - tokenStart, 0));
        var isNegated = fragment.StartsWith("!", StringComparison.Ordinal);
        if (isNegated)
            fragment = fragment[1..];

        if (valueCompletionProvider is not null
            && TryCreateValueCompletionContext(text, caret, tokenStart, tokenEnd, isNegated, out var valueContext))
        {
            var valueCompletions = valueCompletionProvider(valueContext);
            if (valueCompletions is { Count: > 0 })
            {
                var filteredValueCompletions = new List<FilterCompletionItem>();
                AppendCompletionDefinitions(
                    valueCompletions,
                    valueContext.FragmentSpan.Slice(text),
                    valueContext.ValueSpan,
                    filteredValueCompletions,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                if (filteredValueCompletions.Count > 0)
                    return filteredValueCompletions;
            }
        }

        var followsLeftParenthesis = IsAfterLeftParenthesis(text, tokenStart);
        var completionFragment = isNegated || followsLeftParenthesis ? [] : fragment;

        var completions = new List<FilterCompletionItem>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AppendIntrinsicCompletions(completionFragment, replacementSpan, completions, seen);

        if (_defaultTextMatch is { Syntax.ExampleValue: { Length: > 0 } example } && StartsWith(example, completionFragment))
        {
            var insertText = isNegated ? $"!{example}" : example;
            if (seen.Add(CreateCompletionGroupKey(_defaultTextMatch)))
                completions.Add(new(example, insertText, replacementSpan, _defaultTextMatch.Description));
        }

        foreach (var match in _matches)
        {
            var completion = match.CompletionText;
            if (string.IsNullOrEmpty(completion) || !StartsWith(completion, completionFragment))
                continue;

            var insertText = isNegated && match.Syntax.Role is not FilterTermRole.ViewRange
                ? $"!{completion}"
                : completion;
            if (seen.Add(CreateCompletionGroupKey(match)))
                completions.Add(new(completion, insertText, replacementSpan, match.Description));
        }

        return completions;
    }

    /// <summary>
    /// 生成用于 completion 去重的分组键。
    /// </summary>
    private static string CreateCompletionGroupKey(FilterSyntaxMatch match)
        => $"{match.Syntax.Key}|{match.Metadata}";

    /// <summary>
    /// 将外部注入的固定补全项追加到当前补全列表中。
    /// </summary>
    private void AppendIntrinsicCompletions(
        ReadOnlySpan<char> fragment,
        FilterTextSpan replacementSpan,
        ICollection<FilterCompletionItem> completions,
        ISet<string> seen)
        => AppendCompletionDefinitions(_intrinsicCompletions, fragment, replacementSpan, completions, seen);

    /// <summary>
    /// 把补全定义过滤并投影为最终补全项。
    /// </summary>
    private static void AppendCompletionDefinitions(
        IReadOnlyCollection<FilterCompletionDefinition> definitions,
        ReadOnlySpan<char> fragment,
        FilterTextSpan replacementSpan,
        ICollection<FilterCompletionItem> completions,
        ISet<string> seen)
    {
        foreach (var completion in definitions)
        {
            if (!StartsWith(completion.DisplayText, fragment) && !StartsWith(completion.InsertText, fragment))
                continue;

            if (seen.Add(completion.Key))
                completions.Add(new(completion.DisplayText, completion.InsertText, replacementSpan, completion.Description));
        }
    }

    /// <summary>
    /// 判断当前 token 是否已经进入某个带值语法的值区域。
    /// </summary>
    private bool TryCreateValueCompletionContext(
        string text,
        int caret,
        int tokenStart,
        int tokenEnd,
        bool isNegated,
        out FilterValueCompletionContext context)
    {
        var tokenOffset = isNegated ? 1 : 0;
        var headerStart = tokenStart + tokenOffset;
        if (headerStart >= tokenEnd)
        {
            context = null!;
            return false;
        }

        var token = text.AsSpan(headerStart, tokenEnd - headerStart);
        foreach (var match in _matches)
        {
            if (match.Syntax.ValueKind is FilterValueKind.None
                || !token.StartsWith(match.HeaderText, StringComparison.OrdinalIgnoreCase))
                continue;

            var valueStart = headerStart + match.HeaderText.Length;
            if (caret < valueStart)
                break;

            context = new(
                match,
                text,
                FilterTextSpan.FromBounds(tokenStart, tokenEnd),
                FilterTextSpan.FromBounds(valueStart, tokenEnd),
                FilterTextSpan.FromBounds(valueStart, caret),
                isNegated);
            return true;
        }

        context = null!;
        return false;
    }

    /// <summary>
    /// 判断当前 token 是否紧跟在左括号之后。
    /// </summary>
    private static bool IsAfterLeftParenthesis(string text, int tokenStart)
    {
        for (var index = tokenStart - 1; index >= 0; --index)
        {
            if (char.IsWhiteSpace(text[index]))
                continue;

            return text[index] is '(';
        }

        return false;
    }

    /// <summary>
    /// 查找当前 token 的起始位置。
    /// </summary>
    private static int FindTokenStart(string text, int caret)
    {
        var index = caret;
        while (index > 0)
        {
            var previous = text[index - 1];
            if (char.IsWhiteSpace(previous) || previous is '(' or ')')
                break;

            --index;
        }

        return index;
    }

    /// <summary>
    /// 查找当前 token 的结束位置。
    /// </summary>
    private static int FindTokenEnd(string text, int caret)
    {
        var index = caret;
        while (index < text.Length)
        {
            var current = text[index];
            if (char.IsWhiteSpace(current) || current is '(' or ')')
                break;

            ++index;
        }

        return index;
    }

    /// <summary>
    /// 判断补全文本是否以前缀片段开头。
    /// </summary>
    private static bool StartsWith(string completion, ReadOnlySpan<char> fragment)
        => fragment.IsEmpty || completion.StartsWith(fragment, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 实际执行词法读取和语法解析的内部解析器。
    /// </summary>
    private sealed class Parser(FilterLanguage language, string text)
    {
        private int _position;
        private bool _viewRangeAssigned;
        private Range _viewRange = Range.All;
        private readonly List<FilterDiagnostic> _diagnostics = [];

        public IReadOnlyList<FilterDiagnostic> Diagnostics => _diagnostics;

        /// <summary>
        /// 解析完整的过滤文本。
        /// </summary>
        public FilterQuery? Parse()
        {
            var children = ParseTerms(expectRightParenthesis: false, groupStart: -1);
            SkipWhiteSpace();
            if (_diagnostics.Count is 0 && !IsAtEnd)
                AddUnexpectedTokenDiagnostic(CurrentTokenSpan());

            return _diagnostics.Count is 0
                ? new(new(FilterLogicalOperator.And, children.ToArray(), FilterTextSpan.FromBounds(0, text.Length)), _viewRangeAssigned ? _viewRange : Range.All)
                : null;
        }

        /// <summary>
        /// 解析当前层级中的连续条件列表。
        /// </summary>
        private List<FilterNode> ParseTerms(bool expectRightParenthesis, int groupStart)
        {
            var children = new List<FilterNode>();
            while (true)
            {
                SkipWhiteSpace();
                if (IsAtEnd)
                {
                    if (expectRightParenthesis)
                        AddDiagnostic(
                            FilterDiagnosticKind.MissingRightParenthesis,
                            FilterTextSpan.FromBounds(groupStart, Math.Min(groupStart + 1, text.Length)),
                            "(");
                    break;
                }

                if (Current is ')')
                    break;

                if (!TryParseTerm(out var node))
                    break;

                if (node is not null)
                    children.Add(node);
            }

            return children;
        }

        /// <summary>
        /// 解析单个条件或逻辑分组。
        /// </summary>
        private bool TryParseTerm(out FilterNode? node)
        {
            node = null;
            var termStart = _position;
            var isNegated = false;
            if (Current is '!')
            {
                isNegated = true;
                ++_position;
                SkipWhiteSpace();
                if (IsAtEnd)
                {
                    AddDiagnostic(FilterDiagnosticKind.MissingPredicateAfterNegation, FilterTextSpan.FromBounds(termStart, _position), "!");
                    return false;
                }
            }

            if (Current is '(')
            {
                node = ParseGroup(termStart, isNegated);
                return node is not null;
            }

            if (TryParseSyntaxTerm(termStart, isNegated, out node))
                return _diagnostics.Count is 0;

            if (_diagnostics.Count > 0)
                return false;

            if (language._defaultTextMatch is { } defaultText)
            {
                if (!TryParseTextValue(defaultText, defaultText.HeaderText.Length is 0 ? termStart : _position, out var rawValue))
                    return false;

                var termSpan = FilterTextSpan.FromBounds(termStart, _position);
                if (!defaultText.Syntax.TryBind(defaultText, rawValue, termSpan, out var boundValue, out var diagnostic))
                {
                    _diagnostics.Add(diagnostic!);
                    return false;
                }

                node = new FilterPredicateNode(defaultText.Syntax, boundValue, termSpan, isNegated);
                return true;
            }

            AddUnexpectedTokenDiagnostic(CurrentTokenSpan());
            return false;
        }

        /// <summary>
        /// 解析一个 and/or 分组。
        /// </summary>
        private FilterGroupNode? ParseGroup(int termStart, bool isNegated)
        {
            var groupStart = _position;
            ++_position;
            SkipWhiteSpace();
            var operatorStart = _position;
            var groupOperator = ParseGroupOperator(operatorStart);
            if (groupOperator is null)
                return null;

            var children = ParseTerms(expectRightParenthesis: true, groupStart);
            if (_diagnostics.Count > 0)
                return null;

            if (IsAtEnd || Current is not ')')
                return null;

            ++_position;
            return new(groupOperator.Value, [.. children], FilterTextSpan.FromBounds(termStart, _position), isNegated);
        }

        /// <summary>
        /// 读取分组开头的逻辑操作符。
        /// </summary>
        private FilterLogicalOperator? ParseGroupOperator(int operatorStart)
        {
            var wordStart = _position;
            while (!IsAtEnd && char.IsLetter(Current))
                ++_position;

            var word = text.AsSpan(wordStart, _position - wordStart);
            if (word.Equals("and", StringComparison.OrdinalIgnoreCase))
                return FilterLogicalOperator.And;
            if (word.Equals("or", StringComparison.OrdinalIgnoreCase))
                return FilterLogicalOperator.Or;

            AddDiagnostic(
                FilterDiagnosticKind.MissingGroupOperator,
                FilterTextSpan.FromBounds(operatorStart, Math.Max(operatorStart + Math.Max(_position - operatorStart, 1), operatorStart + 1)),
                GetDiagnosticArgument(FilterTextSpan.FromBounds(wordStart, Math.Max(_position, wordStart + 1))));
            return null;
        }

        /// <summary>
        /// 按已注册语法解析当前条件头和其绑定值。
        /// </summary>
        private bool TryParseSyntaxTerm(int termStart, bool isNegated, out FilterNode? node)
        {
            node = null;
            var candidates = language._matchesByFirstChar.TryGetValue(char.ToUpperInvariant(Current), out var matches)
                ? matches
                : [];
            var match = candidates.FirstOrDefault(currentMatch => text.AsSpan(_position).StartsWith(currentMatch.HeaderText, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                if (language._specialStarters.Contains(Current) || Current is ']' or ')')
                    AddUnexpectedTokenDiagnostic(CurrentTokenSpan());
                return false;
            }

            _position += match.HeaderText.Length;
            SkipWhiteSpace();
            if (!TryParseValue(match, termStart, out var rawValue))
                return false;

            var termSpan = FilterTextSpan.FromBounds(termStart, _position);
            if (!match.Syntax.TryBind(match, rawValue, termSpan, out var boundValue, out var diagnostic))
            {
                _diagnostics.Add(diagnostic!);
                return false;
            }

            if (match.Syntax.Role is FilterTermRole.ViewRange)
            {
                if (isNegated)
                {
                    AddDiagnostic(FilterDiagnosticKind.UnsupportedNegation, termSpan, match.DiagnosticText, "!");
                    return false;
                }

                if (_viewRangeAssigned)
                {
                    AddDiagnostic(FilterDiagnosticKind.DuplicateViewRange, termSpan, match.DiagnosticText);
                    return false;
                }

                if (boundValue is not Range viewRange)
                {
                    AddDiagnostic(FilterDiagnosticKind.InternalViewRangeBindingFailed, termSpan, match.DiagnosticText);
                    return false;
                }

                _viewRange = viewRange;
                _viewRangeAssigned = true;
                return true;
            }

            node = new FilterPredicateNode(match.Syntax, boundValue, termSpan, isNegated);
            return true;
        }

        /// <summary>
        /// 根据语法声明选择对应的值解析器。
        /// </summary>
        private bool TryParseValue(FilterSyntaxMatch match, int termStart, out FilterValue value)
        {
            switch (match.Syntax.ValueKind)
            {
                case FilterValueKind.None:
                    value = new FilterNoneValue(FilterTextSpan.EmptyAt(_position));
                    return true;
                case FilterValueKind.Text:
                    return TryParseTextValue(match, termStart, out value);
                case FilterValueKind.LongRange:
                    return TryParseLongRangeValue(match, out value);
                case FilterValueKind.DoubleRange:
                    return TryParseDoubleRangeValue(match, out value);
                case FilterValueKind.Date:
                    return TryParseDateValue(match, out value);
                default:
                    value = new FilterNoneValue(FilterTextSpan.EmptyAt(_position));
                    AddDiagnostic(
                        FilterDiagnosticKind.UnsupportedValueKind,
                        FilterTextSpan.FromBounds(termStart, _position),
                        match.DiagnosticText,
                        match.Syntax.ValueKind);
                    return false;
            }
        }

        /// <summary>
        /// 读取字符串值，支持引号和末尾精确匹配标记。
        /// </summary>
        private bool TryParseTextValue(FilterSyntaxMatch match, int termStart, out FilterValue value)
        {
            if (IsAtEnd || Current is ')')
            {
                value = new FilterNoneValue(FilterTextSpan.EmptyAt(_position));
                AddDiagnostic(FilterDiagnosticKind.MissingTextValue, FilterTextSpan.FromBounds(termStart, _position), match.DiagnosticText);
                return false;
            }

            if (Current is '"')
            {
                var quotedStart = _position;
                ++_position;
                var contentStart = _position;
                while (!IsAtEnd && Current is not '"')
                    ++_position;

                if (IsAtEnd)
                {
                    value = new FilterNoneValue(FilterTextSpan.FromBounds(quotedStart, _position));
                    AddDiagnostic(
                        FilterDiagnosticKind.MissingStringQuote,
                        FilterTextSpan.FromBounds(quotedStart, _position),
                        GetDiagnosticArgument(FilterTextSpan.FromBounds(quotedStart, _position)));
                    return false;
                }

                var contentEnd = _position;
                ++_position;
                var isExact = TryConsume('$');
                value = new FilterTextValue(
                    text,
                    FilterTextSpan.FromBounds(contentStart, contentEnd),
                    isExact,
                    FilterTextSpan.FromBounds(quotedStart, _position));
                return true;
            }

            var start = _position;
            while (!IsAtEnd && !char.IsWhiteSpace(Current) && Current is not ')')
                ++_position;

            var end = _position;
            var isExactUnquoted = end > start && text[end - 1] is '$';
            var contentEndUnquoted = isExactUnquoted ? end - 1 : end;
            if (contentEndUnquoted <= start)
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(start, end));
                AddDiagnostic(FilterDiagnosticKind.MissingTextValue, FilterTextSpan.FromBounds(termStart, end), match.DiagnosticText);
                return false;
            }

            value = new FilterTextValue(
                text,
                FilterTextSpan.FromBounds(start, contentEndUnquoted),
                isExactUnquoted,
                FilterTextSpan.FromBounds(start, end));
            return true;
        }

        /// <summary>
        /// 读取整数范围，仅保留 a-b、-b、a- 三种简单写法。
        /// </summary>
        private bool TryParseLongRangeValue(FilterSyntaxMatch match, out FilterValue value)
        {
            var rangeStart = _position;
            if (IsAtEnd || Current is ')')
            {
                value = new FilterNoneValue(FilterTextSpan.EmptyAt(_position));
                AddDiagnostic(FilterDiagnosticKind.MissingRangeValue, FilterTextSpan.FromBounds(rangeStart, _position), match.DiagnosticText);
                return false;
            }

            if (Current is '[' or '(')
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position + 1));
                AddDiagnostic(
                    FilterDiagnosticKind.InvalidLongRangeFormat,
                    FilterTextSpan.FromBounds(rangeStart, _position + 1),
                    match.DiagnosticText,
                    GetDiagnosticArgument(FilterTextSpan.FromBounds(rangeStart, _position + 1)));
                return true;
            }

            if (Current is '-')
            {
                ++_position;
                SkipWhiteSpace();
                if (!TryReadULong(out var upper, out _))
                {
                    value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position));
                    return false;
                }

                value = new FilterRawLongRangeValue(new(null, true, upper, true), FilterTextSpan.FromBounds(rangeStart, _position));
                return true;
            }

            if (!TryReadULong(out var lowerBound, out _))
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position));
                return false;
            }

            SkipWhiteSpace();
            if (!TryConsume('-'))
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position));
                AddDiagnostic(
                    FilterDiagnosticKind.InvalidLongRangeFormat,
                    FilterTextSpan.FromBounds(rangeStart, _position),
                    match.DiagnosticText,
                    GetDiagnosticArgument(FilterTextSpan.FromBounds(rangeStart, _position)));
                return false;
            }

            SkipWhiteSpace();
            if (IsAtEnd || Current is ')')
            {
                value = new FilterRawLongRangeValue(new(lowerBound, true, null, true), FilterTextSpan.FromBounds(rangeStart, _position));
                return true;
            }

            if (!TryReadULong(out var upperBound, out _))
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position));
                return false;
            }

            value = new FilterRawLongRangeValue(new(lowerBound, true, upperBound, true), FilterTextSpan.FromBounds(rangeStart, _position));
            return true;
        }

        /// <summary>
        /// 读取小数范围，支持整数、小数和分数形式。
        /// </summary>
        private bool TryParseDoubleRangeValue(FilterSyntaxMatch match, out FilterValue value)
        {
            var rangeStart = _position;
            if (IsAtEnd || Current is ')')
            {
                value = new FilterNoneValue(FilterTextSpan.EmptyAt(_position));
                AddDiagnostic(FilterDiagnosticKind.MissingRangeValue, FilterTextSpan.FromBounds(rangeStart, _position), match.DiagnosticText);
                return false;
            }

            if (Current is '[' or '(')
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position + 1));
                AddDiagnostic(
                    FilterDiagnosticKind.InvalidDoubleRangeFormat,
                    FilterTextSpan.FromBounds(rangeStart, _position + 1),
                    match.DiagnosticText,
                    GetDiagnosticArgument(FilterTextSpan.FromBounds(rangeStart, _position + 1)));
                return false;
            }

            if (Current is '-')
            {
                ++_position;
                SkipWhiteSpace();
                if (!TryReadDoubleLiteral(out var upper, out _))
                {
                    value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position));
                    return false;
                }

                value = new FilterRawDoubleRangeValue(new(null, true, upper, true), FilterTextSpan.FromBounds(rangeStart, _position));
                return true;
            }

            if (!TryReadDoubleLiteral(out var lower, out _))
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position));
                return false;
            }

            SkipWhiteSpace();
            if (!TryConsume('-'))
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position));
                AddDiagnostic(
                    FilterDiagnosticKind.InvalidDoubleRangeFormat,
                    FilterTextSpan.FromBounds(rangeStart, _position),
                    match.DiagnosticText,
                    GetDiagnosticArgument(FilterTextSpan.FromBounds(rangeStart, _position)));
                return false;
            }

            SkipWhiteSpace();
            if (IsAtEnd || Current is ')')
            {
                value = new FilterRawDoubleRangeValue(new(lower, true, null, true), FilterTextSpan.FromBounds(rangeStart, _position));
                return true;
            }

            if (!TryReadDoubleLiteral(out var upperBound, out _))
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(rangeStart, _position));
                return false;
            }

            value = new FilterRawDoubleRangeValue(new(lower, true, upperBound, true), FilterTextSpan.FromBounds(rangeStart, _position));
            return true;
        }

        /// <summary>
        /// 读取日期值，支持 yyyy-M-d 和 M-d 两种形式。
        /// </summary>
        private bool TryParseDateValue(FilterSyntaxMatch match, out FilterValue value)
        {
            var dateStart = _position;
            if (IsAtEnd || Current is ')')
            {
                value = new FilterNoneValue(FilterTextSpan.EmptyAt(_position));
                AddDiagnostic(FilterDiagnosticKind.MissingDateValue, FilterTextSpan.FromBounds(dateStart, _position), match.DiagnosticText);
                return false;
            }

            if (!TryReadULong(out var first, out _))
            {
                value = new FilterNoneValue(FilterTextSpan.EmptyAt(_position));
                return false;
            }

            if (!TryConsumeDateSeparator())
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(dateStart, _position));
                AddDiagnostic(
                    FilterDiagnosticKind.DateRequiresMonthAndDay,
                    FilterTextSpan.FromBounds(dateStart, _position),
                    match.DiagnosticText,
                    GetDiagnosticArgument(FilterTextSpan.FromBounds(dateStart, _position)));
                return false;
            }

            if (!TryReadULong(out var second, out _))
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(dateStart, _position));
                return false;
            }

            if (TryConsumeDateSeparator())
            {
                if (!TryReadULong(out var third, out _)
                    || !TryNarrow(first, out var year)
                    || !TryNarrow(second, out var month)
                    || !TryNarrow(third, out var day))
                {
                    value = new FilterNoneValue(FilterTextSpan.FromBounds(dateStart, _position));
                    return false;
                }

                value = new FilterRawDateValue(new(year, month, day), FilterTextSpan.FromBounds(dateStart, _position));
                return true;
            }

            if (!TryNarrow(first, out var inferredMonth) || !TryNarrow(second, out var inferredDay))
            {
                value = new FilterNoneValue(FilterTextSpan.FromBounds(dateStart, _position));
                return false;
            }

            value = new FilterRawDateValue(new(null, inferredMonth, inferredDay), FilterTextSpan.FromBounds(dateStart, _position));
            return true;
        }

        /// <summary>
        /// 读取一个无符号整数文本片段。
        /// </summary>
        private bool TryReadULong(out long value, out FilterTextSpan span)
        {
            var start = _position;
            while (!IsAtEnd && char.IsDigit(Current))
                ++_position;

            if (start == _position)
            {
                span = FilterTextSpan.EmptyAt(_position);
                value = 0;
                AddDiagnostic(FilterDiagnosticKind.ExpectedInteger, span, GetDiagnosticArgument(CurrentTokenSpan()));
                return false;
            }

            span = FilterTextSpan.FromBounds(start, _position);
            if (!long.TryParse(text.AsSpan(start, _position - start), NumberStyles.None, CultureInfo.InvariantCulture, out value))
            {
                AddDiagnostic(FilterDiagnosticKind.IntegerOutOfRange, span, GetDiagnosticArgument(span));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 读取一个比例或小数字面量。
        /// </summary>
        private bool TryReadDoubleLiteral(out double value, out FilterTextSpan span)
        {
            var literalStart = _position;
            if (!TryReadULong(out var integral, out _))
            {
                span = FilterTextSpan.EmptyAt(_position);
                value = 0;
                return false;
            }

            if (!IsAtEnd && Current is '/')
            {
                ++_position;
                if (!TryReadULong(out var denominator, out _))
                {
                    span = FilterTextSpan.FromBounds(literalStart, _position);
                    value = 0;
                    return false;
                }

                if (denominator is 0)
                {
                    span = FilterTextSpan.FromBounds(literalStart, _position);
                    value = 0;
                    AddDiagnostic(FilterDiagnosticKind.DenominatorCannotBeZero, span, GetDiagnosticArgument(span));
                    return false;
                }

                span = FilterTextSpan.FromBounds(literalStart, _position);
                value = (double)integral / denominator;
                return true;
            }

            if (!IsAtEnd && Current is '.')
            {
                ++_position;
                var fractionalStart = _position;
                while (!IsAtEnd && char.IsDigit(Current))
                    ++_position;

                if (fractionalStart == _position)
                {
                    span = FilterTextSpan.FromBounds(literalStart, _position);
                    value = 0;
                    AddDiagnostic(FilterDiagnosticKind.MissingFractionalPart, span, GetDiagnosticArgument(span));
                    return false;
                }

                span = FilterTextSpan.FromBounds(literalStart, _position);
                if (!double.TryParse(text.AsSpan(literalStart, _position - literalStart), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    AddDiagnostic(FilterDiagnosticKind.InvalidDoubleValue, span, GetDiagnosticArgument(span));
                    return false;
                }

                return true;
            }

            span = FilterTextSpan.FromBounds(literalStart, _position);
            value = integral;
            return true;
        }

        /// <summary>
        /// 如果当前位置命中指定字符则向前消费一个字符。
        /// </summary>
        private bool TryConsume(char ch)
        {
            if (!IsAtEnd && Current == ch)
            {
                ++_position;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试消费日期中的分隔符。
        /// </summary>
        private bool TryConsumeDateSeparator()
        {
            if (IsAtEnd || Current is not '-' and not '.')
                return false;

            ++_position;
            return true;
        }

        /// <summary>
        /// 将 long 值安全地缩窄为 int。
        /// </summary>
        private bool TryNarrow(long value, out int narrowed)
        {
            if (value > int.MaxValue)
            {
                narrowed = 0;
                AddDiagnostic(FilterDiagnosticKind.DateValueTooLarge, CurrentTokenSpan(), value);
                return false;
            }

            narrowed = (int)value;
            return true;
        }

        /// <summary>
        /// 跳过当前位置之后的连续空白字符。
        /// </summary>
        private void SkipWhiteSpace()
        {
            while (!IsAtEnd && char.IsWhiteSpace(Current))
                ++_position;
        }

        /// <summary>
        /// 获取当前位置附近 token 的可视区间。
        /// </summary>
        private FilterTextSpan CurrentTokenSpan()
        {
            var start = _position;
            var end = start;
            while (end < text.Length && !char.IsWhiteSpace(text[end]) && text[end] is not '(' and not ')')
                ++end;

            if (end == start)
                end = Math.Min(start + 1, text.Length);

            return FilterTextSpan.FromBounds(start, end);
        }

        /// <summary>
        /// 记录一条结构化过滤诊断。
        /// </summary>
        private void AddDiagnostic(FilterDiagnosticKind kind, FilterTextSpan span, params IReadOnlyList<object?> arguments)
            => _diagnostics.Add(new(kind, span, arguments));

        private void AddUnexpectedTokenDiagnostic(FilterTextSpan span)
            => AddDiagnostic(FilterDiagnosticKind.UnexpectedToken, span, GetDiagnosticArgument(span));

        private string GetDiagnosticArgument(FilterTextSpan span)
        {
            var value = span.GetText(text);
            return value.Length > 0 ? value : "";
        }

        private bool IsAtEnd => _position >= text.Length;

        private char Current => text[_position];
    }
}
