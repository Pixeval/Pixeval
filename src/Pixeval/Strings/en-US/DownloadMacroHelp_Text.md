Pixeval provides Path Macros to fine tune download paths.
Each Path Macro is in form of `@{name}`. In case of providing parameters, it should be like `@{name=...}`.\
每个路径宏的形式均为 `@{name}`，有些有参数的宏则是 `@{name=...}`。

While downloading illustrations, these macros will be substituted by corresponding texts, e.g., `@{id}` will be replaced by the id of artwork.

If a macro provides a parameter, then it is a conditional macro, which will be substituted if a certain condition holds.
For example, while downloading a manga, `@{if_pic_set=\MANGA\}` will be substituted by `\MANGA\`.\
例如如果正在下载一副漫画作品，则 `@{if_pic_set=\漫画\}` 会被替换为 `\漫画\`。

By the way, conditional macros can be nested, like `@{if_pic_set=...@{if_r18=...}...}`.
Negations could also be used, like `@{!if_pic_set=...}` which filters out mangas.\
条件宏也可以使用反模式，例如 `@{!if_pic_set=...}` 表示不是漫画。

Some macros should satisfy special constraints:

- `@{pic_set_index}` must be used in conditional macro `@{if_pic_set=...}`.
- `@{pic_set_index}` and `@{ext}` must be used in file names.

You can move your cursor on a button to see how the corresponding macro works.
