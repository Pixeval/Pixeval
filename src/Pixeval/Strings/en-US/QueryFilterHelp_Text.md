The complete tutorial mainly focuses on grammar illustration with several examples, aims to provide all kinds of queries.

## Syntax of query

| Syntax                                                                             | Meaning                                               |
| ---------------------------------------------------------------------------------- | ----------------------------------------------------- |
| [str]                          | Title of artwork                                      |
| #[str]                         | Tag                                                   |
| @[str \\| num]   | Author                                                |
| +[const]                       | Positive constraint                                   |
| -[const]                       | Negative constraint                                   |
| i:[int-range]  | Filter artworks by their indices                      |
| l:[int-range]  | Filter artworks by their numbers of likes             |
| r:[frac-range] | Filter illustrations by ratio, novels will be ignored |
| s:[date]       | Dates of publication from                             |
| e:[date]       | Dates fo publication to                               |

## Syntax of values

### [str] Strings

| Syntax  | Meaning                                                      |
| ------- | ------------------------------------------------------------ |
| abc     | A simple string                                              |
| "ab# c" | A string with spaces/special characters                      |
| abc$    | A string that exactly matches                                |
| "ab c$" | A string that exactly matches with spaces/special characters |

### [num] Numbers

| Syntax | Meaning           |
| ------ | ----------------- |
| 12345  | Ordinary integers |

### [const] Constraints

| Syntax | Meaning                      |
| ------ | ---------------------------- |
| r18    | R18 contents, including R18G |
| r18g   | R18G contents                |
| gif    | GIF Animations               |
| ai     | AI-generated contents        |

### [int-range] Range/Intervals of positive integer

| Syntax                                                    | Meaning                                                                                       |
| --------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| 2-                                                        | Greater than or equals to 2                                                                   |
| -3                                                        | Smaller than or equals to 3                                                                   |
| 2-3                                                       | Between 2 and 3, inclusive                                                                    |
| [2,3] | Interval style, between 2 and 3, inclusive                                                    |
| \[2,3)                         | Interval style, between 2 (inclusive) and 3 (exclusive) |

> Interval style doesn't support half-opened infinitive intervals like '2-' or '-3'.

### [frac-range] Range of positive decimals

| Syntax                  | Meaning                                                         |
| ----------------------- | --------------------------------------------------------------- |
| 2-                      | Greater than or equals to 2                                     |
| -1.5    | Lesser or equals to 1.5                         |
| -1/2                    | Lesser or equals to 1/2                                         |
| 1/2-3                   | Between 1/2 and 3, inclusive                                    |
| 0.3-1/2 | Larger than 0.3 and smaller than 1/2, inclusive |

### [date] Date

| Syntax                                     | Meaning                                |
| ------------------------------------------ | -------------------------------------- |
| MM-dd                                      | Month of MM and day of dd of this year |
| MM.dd                      | Month of MM and day of dd of this year |
| yyyy-MM-dd                                 | Month of MM and day of dd of year yyyy |
| yyyy.MM.dd | Month of MM and day of dd of year yyyy |

> '.' and '-' can be used in mixture.

## Combinations

| Syntax                                           | Meaning                                     |
| ------------------------------------------------ | ------------------------------------------- |
| !\<segment>                                     | Negation                                    |
| (and \<segment> \<segment>) | And pattern, all segments must be satisfied |
| (or \<segment> \<segment>)  | Or pattern, any segment can be satisfied    |

> These three syntaxes could be nested in any combination. The top level combination uses the 'and' pattern.
