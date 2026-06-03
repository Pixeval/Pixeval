// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Filters.Analysis;

/// <summary>
/// 描述过滤解析阶段可能出现的诊断类型。
/// </summary>
public enum FilterDiagnosticKind
{
    UnexpectedToken,
    MissingPredicateAfterNegation,
    MissingTextValue,
    MissingLongValue,
    MissingDoubleValue,
    MissingRangeValue,
    MissingDateValue,
    MissingGroupOperator,
    MissingRightParenthesis,
    InvalidValue,
    MissingStringQuote,
    InvalidLongRangeFormat,
    InvalidDoubleRangeFormat,
    DateRequiresMonthAndDay,
    ExpectedInteger,
    IntegerOutOfRange,
    DenominatorCannotBeZero,
    MissingFractionalPart,
    InvalidDoubleValue,
    DateValueTooLarge,
    NegativeRangeUnsupported,
    RangeMinimumGreaterThanMaximum,
    DoubleRangeOpenIntervalUnsupported,
    InvalidDate,
    UnsupportedValueKind,
    InternalExpectedTextValue,
    InternalExpectedLongValue,
    InternalExpectedDoubleValue,
    InternalExpectedLongRangeValue,
    InternalExpectedDoubleRangeValue,
    InternalExpectedDateValue
}
