## Syntax of query

| Syntax        | Meaning                                   |
|---------------|-------------------------------------------|
| [str]         | Title of artwork                          |
| #[str]        | Tag                                       |
| @[str \| num] | Author                                    |
| +[const]      | Positive constraint                       |
| -[const]      | Negative constraint                       |
| i:[range]     | Filter artworks by their indices          |
| l:[range]     | Filter artworks by their numbers of likes |
| s:[date]      | Dates of publication from                 |
| e:[date]      | Dates fo publication to                   |

## Syntax of values

### [str] Strings 

| Syntax  | Meaning                                                      |
|---------|--------------------------------------------------------------|
| abc     | A simple string                                              |
| "ab# c" | A string with spaces/special characters                      |
| abc$    | A string that exactly matches                                |
| "ab c$" | A string that exactly matches with spaces/special characters |

### [num] Numbers

| Syntax | Meaning           |
|--------|-------------------|
| 12345  | Ordinary integers |

### [const] Constraints

| Syntax | Meaning                      |
|--------|------------------------------|
| r18    | R18 contents, including R18G |
| r18g   | R18G contents                |
| gif    | GIF Animations               |
| ai     | AI-generated contents        |

### [range] Range/Intervals

| Syntax | Meaning                                                 |
|--------|---------------------------------------------------------|
| 2-     | Greater than or equals to 2                             |
| -3     | Smaller than or equals to 3                             |
| 2-3    | Between 2 and 3, inclusive                              |
| [2,3]  | Interval style, between 2 and 3, inclusive              |
| [2,3)  | Interval style, between 2 (inclusive) and 3 (exclusive) |

> Interval style doesn't support half-opened infinitive intervals like '2-' or '-3'.

### [date] | Date

| Syntax     | Meaning                                |
|------------|----------------------------------------|
| MM-dd      | Month of MM and day of dd of this year |
| MM.dd      | Month of MM and day of dd of this year |
| yyyy-MM-dd | Month of MM and day of dd of year yyyy |
| yyyy.MM.dd | Month of MM and day of dd of year yyyy |

'.' and '-' can be used in mixture.

## Combinations

| Syntax                    | Meaning                                     |
|---------------------------|---------------------------------------------|
| !<segment>                | Negation                                    |
| (and <segment> <segment>) | And pattern, all segments must be satisfied |
| (or <segment> <segment>)  | Or pattern, any segment can be satisfied    |

> These three syntaxes could be nested in any combination. The top level combination uses the 'and' pattern.
