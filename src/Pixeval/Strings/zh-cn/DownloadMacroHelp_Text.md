Pixeval 提供路径宏用于更精细的设定下载路径。  
每个路径宏的形式均为 `@{name}`，有些有参数的宏则是 `@{name=...}`。

在下载图片时，这些宏会被替换为对应的文本，例如，`@{id}` 宏会在下载时被自动替换为作品ID。

如果一个宏带有参数，则说明是一个条件宏，该宏会在条件满足时被替换为其参数内容。  
例如如果正在下载一副漫画作品，则 `@{if_pic_set=\漫画\}` 会被替换为 `\漫画\`。

此外，条件宏可以嵌套使用，例如 `@{if_pic_set=...@{if_r18=...}...}`。  
条件宏也可以使用反模式，例如 `@{!if_pic_set=...}` 表示不是漫画。

有些宏有些特殊约束：
- `@{pic_set_index}` 必须在条件宏 `@{if_pic_set=...}` 中使用。
- `@{pic_set_index}` 和 `@{ext}` 必须在文件名中使用。

要查看每个宏的作用，请将鼠标指针移动到对应宏的按钮上。
