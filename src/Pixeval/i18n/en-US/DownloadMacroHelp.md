Pixeval provides macros for more precise control over download paths.  
To see what each macro does, hover your mouse over the corresponding macro's button.  
You can move your cursor on a button to see how the corresponding macro works.

## Regular Macros

Regular macros have the form `@{xxx}` or `@{xxx:<format parameter>}` to specify the output format.

When downloading works, these macros are replaced with the corresponding text. For example, the `@{id}` macro is automatically replaced with the work ID when downloading.

### Special Constraints

Some macros should satisfy special constraints:

- `@{series_id}` and `@{series_title}` must be used in the true branch of the conditional macro `@{is_series?:}`.
- `@{pic_set_index}` must be used in the true branch of the conditional macro `@{is_pic_set?:}`.
- `@{group_id}` outputs the subscription download group ID and must be used in the true branch of one of the conditional macros `@{is_group?:}`, `@{is_bookmark_group?:}`, `@{is_post_group?:}`, or `@{is_series_group?:}`.
- `@{pic_set_index}` and `@{ext}` must be used in file names.
- `@{ext}` does not include the leading dot of the extension, e.g. it outputs `jpg` rather than `.jpg`; for the full extension, write `.@{ext}`.

### Format Parameters

#### Strings

For macros with string output, the format parameter can be `u` or `l`, where `u` converts the output to uppercase and `l` converts it to lowercase.

#### Numbers

For macros with numeric output, the format parameter can be a .NET standard numeric format string, for example:

- 0: a zero placeholder, i.e. replaced with the corresponding digit (if present); otherwise a 0 is displayed in the result string.
- \#: a digit placeholder, i.e. replaced with the corresponding digit (if present); otherwise no digit is displayed in the result string.

For example: `@{pic_set_index:000}` formats the image set index as a three-digit number, zero-padded to three digits (000, 001...).

For more usage, see
[Custom numeric format strings](https://learn.microsoft.com/dotnet/standard/base-types/custom-numeric-format-strings)
and [Standard numeric format strings](https://learn.microsoft.com/dotnet/standard/base-types/standard-numeric-format-strings)

> [!WARNING]
> If the formatting contains characters not supported in paths (such as \ / : \* ? " < > |, etc.), they will be automatically removed, which may even cause errors at download time.

#### Dates

For macros with date output, the format parameter can be a .NET standard date and time format string; the default value is `yyyy-M-d`. Common format specifiers include:

- d: day of the month (1 to 31)
- dd: day of the month (01 to 31)
- ddd: abbreviated name of the day of the week (e.g. "Mon")
- dddd: full name of the day of the week (e.g. "Monday")
- M: month (1 to 12)
- MM: month (01 to 12)
- yyyy: year with at least four digits (e.g. 2024)

The less commonly used hours, minutes, seconds, and more usage can be found in
[Custom date and time format strings](https://learn.microsoft.com/dotnet/standard/base-types/custom-date-and-time-format-strings)
and [Standard date and time format strings](https://learn.microsoft.com/dotnet/standard/base-types/standard-date-and-time-format-strings)

> [!WARNING]
> If the formatting contains characters not supported in paths (such as \ / : \* ? " < > |, etc.), they will be automatically removed, which may even cause errors at download time.

## Conditional Macros

If a macro has conditional branches, it is a conditional macro. Conditional macros use a ternary-operator-like form: `@{is_xxx?<true branch>:<false branch>}`.
:<false branch>}\`.
When the condition is met, the part after the question mark "?" is output; otherwise, the part after the colon ":" is output.

For example, if a novel work is being downloaded, `@{is_novel?Novel:Image}` will be replaced with `Novel`; if it is not a novel work, it will be replaced with `Image`.

In addition, conditional macros can be nested, for example: `@{is_pic_set?...@{is_r18?R18:All Ages}...:Single Image}`.
