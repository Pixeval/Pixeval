Pixeval 提供宏用于更精细的设定下载路径。  
要查看每个宏的作用，请将鼠标指针移动到对应宏的按钮上。

## 普通宏

普通宏的形式为 `@{xxx}` 或 `@{xxx:<格式化参数>}` 指定输出格式。

在下载作品时，这些宏会被替换为对应的文本，例如，`@{id}` 宏会在下载时被自动替换为作品ID。

### 特殊约束

有些宏有些特殊约束：

- `@{series_id}` 和 `@{series_title}` 必须在条件宏 `@{is_series?:}` 的正分支中使用。
- `@{pic_set_index}` 必须在条件宏 `@{is_pic_set?:}` 的正分支中使用。
- `@{group_id}` 会输出订阅下载组 ID，必须在 `@{is_group?:}`、`@{is_bookmark_group?:}`、`@{is_post_group?:}` 或 `@{is_series_group?:}` 任意一个条件宏的正分支中使用。
- `@{pic_set_index}` 和 `@{ext}` 必须在文件名中使用。
- `@{ext}` 不包含扩展名前的点号，例如它会输出 `jpg` 而不是 `.jpg`；需要完整扩展名时应写作 `.@{ext}`。

### 格式化参数

#### 字符串

对于字符串格式的宏，格式化参数可以是 `u` 或者 `l` 两种，其中 `u` 表示将输出全部转换为大写，`l` 则表示转换为小写。

#### 数字

对于数字格式的宏，格式化参数可以是 .NET 标准的数字格式化字符串，例如：

- 0：表示一个零占位符，即用对应的数字（如果存在）替换 0；否则，将在结果字符串中显示 0。
- \#：表示一个数字占位符，即用对应的数字（如果存在）替换 \#；否则，不会在结果字符串中显示任何数字。

更多用法详见
[自定义数字格式字符串](https://learn.microsoft.com/dotnet/standard/base-types/custom-numeric-format-strings)
和 [标准数字格式字符串](https://learn.microsoft.com/dotnet/standard/base-types/standard-numeric-format-strings)

> [!WARNING]
> 若格式化含有路径中不支持的字符（如 \ / : \* ? " < > | 等），则会自动删除，甚至下载出现异常。

#### 日期

对于日期格式的宏，格式化参数可以是 .NET 标准的日期和时间格式字符串，默认值为 `yyyy-M-d`，常用的格式符例如：

- d：一个月中的某一天（1 到 31）
- dd：一个月中的某一天（01 到 31）
- ddd：一周中某天的缩写名称（如“周一”）
- dddd：一周中某天的完整名称（如“星期一”）
- M：月份（1 到 12）
- MM：月份（01 到 12）
- yyyy：至少四位数的年份（如 2024）

不常用的时、分、秒，以及更多用法详见
[自定义日期和时间格式字符串](https://learn.microsoft.com/dotnet/standard/base-types/custom-date-and-time-format-strings)
和 [标准日期和时间格式字符串](https://learn.microsoft.com/dotnet/standard/base-types/standard-date-and-time-format-strings)

> [!WARNING]
> 若格式化含有路径中不支持的字符（如 \ / : \* ? " < > | 等），则会自动删除，甚至下载出现异常。

## 条件宏

如果一个宏带有条件分支，则说明是一个条件宏，条件宏使用 `@{is_xxx?<正分支>:<反分支>}` 这种类似三目运算符的形式。
条件满足时会输出问号“?”后的前半部分，否则输出冒号“:”后的后半部分。

例如如果正在下载一副漫画作品，则 `@{is_novel?小说:图片}` 会被替换为 `小说`；如果不是漫画，则会替换为 `图片`。

此外，条件宏可以嵌套使用，例如 `@{is_pic_set?...@{is_r18?R18:全年龄}...:单图}`。
