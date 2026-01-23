// Copyright (c) Pixeval.Filters.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;

namespace Pixeval.Filters;

public class FilterParseException(FilterParseException.ErrorType type, params IEnumerable<string?> parameter)
    : Exception(type.ToString())
{
    public ErrorType Type { get; } = type;

    public IEnumerable<string?> Parameter { get; } = parameter;

    public enum ErrorType
    {
        FilterTokenFinished,
        UnexpectedToken,
        UnbalancedPar,
        ParserOutOfRange,
        ExpectedAndOrAfterLeftPar,
        IndexRangeUsedMoreThanOnce,
        InvalidConstraint,
        ExpectedRightBracketOrParenInRange,
        NumericTooSmallInRange,
        MinimumShouldBeSmallerThanMaximum,
        ExpectedAtLeastTwoNumericInDate,
        NumericTooLarge
    }
}
