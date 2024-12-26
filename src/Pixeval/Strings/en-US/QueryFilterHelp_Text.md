The complete tutorial mainly focuses on grammar illustration with several examples, aims to provide
all kinds of queries.

## Syntax of query

| Syntax                                                                             | Meaning                                               |
| ---------------------------------------------------------------------------------- | ----------------------------------------------------- |
| [str]                          | Title of artwork                                      |
| #[str]                         | Tag                                                   |
| @[str \\| num]   | Author                                                |
| +[const]                       | Positive constraint                                   |
| -[const]                       | Negative constraint                                   |
| i:[int-range]  | 筛选作品的序号                                               |
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

| Syntax                                                    | Meaning                     |
| --------------------------------------------------------- | --------------------------- |
| 2-                                                        | Greater than or equals to 2 |
| -3                                                        | Smaller than or equals to 3 |
| 2-3                                                       | 大于等于2且小于等于3                 |
| [2,3] | 数学集合，大于等于2且小于等于3            |
| \[2,3)                         | 数学集合，大于等于2且小于3              |

> Interval style doesn't support half-opened infinitive intervals like '2-' or '-3'.

### [frac-range] Range of positive decimals

| Syntax                  | Meaning                                                         |
| ----------------------- | --------------------------------------------------------------- |
| 2-                      | Greater than or equals to 2                                     |
| -1.5    | Lesser or equals to 1.5                         |
| -1/2                    | Lesser or equals to 1/2                                         |
| 1/2-3                   | 大于等于1/2且小于等于3                                                   |
| 0.3-1/2 | Larger than 0.3 and smaller than 1/2, inclusive |

### [date] Date

| Syntax                                     | Meaning |
| ------------------------------------------ | ------- |
| MM-dd                                      | 今年某月某日  |
| MM.dd                      | 今年某月某日  |
| yyyy-MM-dd                                 | 某年某月某日  |
| yyyy.MM.dd | 某年某月某日  |

> '.' and '-' can be used in mixture.

## 序列

| Syntax                                           | Meaning |
| ------------------------------------------------ | ------- |
| !\<segment>                                     | 反模式     |
| (and \<segment> \<segment>) | 和模式     |
| (or \<segment> \<segment>)  | 或模式     |

> 三种模式可以任意嵌套，顶层序列默认使用和模式
